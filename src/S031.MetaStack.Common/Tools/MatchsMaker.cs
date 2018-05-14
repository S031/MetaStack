using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace S031.MetaStack.Common
{
	internal static class MatchsMaker
	{
		public static float GetScore(string left, string right)
		{

			string _lString, _rString;
			string[] _leftTokens, _rightTokens;
			int leftLen, rightLen;
			float[,] cost;

			_lString = left;
			_rString = right;

			_leftTokens = partition(_lString);
			_rightTokens = partition(_rString);
			if (_leftTokens.Length > _rightTokens.Length)
			{
				string[] tmp = _leftTokens;
				_leftTokens = _rightTokens;
				_rightTokens = tmp;
				string s = _lString; _lString = _rString; _rString = s;
			}

			leftLen = _leftTokens.Length - 1;
			rightLen = _rightTokens.Length - 1;

			cost = new float[leftLen + 1, rightLen + 1];
			for (int i = 0; i <= leftLen; i++)
				for (int j = 0; j <= rightLen; j++)
					cost[i, j] = getSimilarity(_leftTokens[i], _rightTokens[j]);

			return new BipartiteMatcher(_leftTokens, _rightTokens, cost).Score;
		}

		static int min3(int a, int b, int c)
		{
			return System.Math.Min(System.Math.Min(a, b), c);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1814:PreferJaggedArraysOverMultidimensional", MessageId = "Body")]
		static int computeDistance(string s, string t)
		{
			int n = s.Length;
			int m = t.Length;
			int[,] distance = new int[n + 1, m + 1]; // matrix
			int cost = 0;

			if (n == 0) return m;
			if (m == 0) return n;
			//init1
			for (int i = 0; i <= n; distance[i, 0] = i++) ;
			for (int j = 0; j <= m; distance[0, j] = j++) ;

			//find min distance
			for (int i = 1; i <= n; i++)
			{
				for (int j = 1; j <= m; j++)
				{
					cost = (t.Substring(j - 1, 1) == s.Substring(i - 1, 1) ? 0 : 1);
					distance[i, j] = min3(distance[i - 1, j] + 1,
						distance[i, j - 1] + 1,
						distance[i - 1, j - 1] + cost);
				}
			}

			return distance[n, m];
		}

		static float getSimilarity(System.String string1, System.String string2)
		{

			float dis = computeDistance(string1, string2);
			float maxLen = string1.Length;
			if (maxLen < (float)string2.Length)
				maxLen = string2.Length;

			float minLen = string1.Length;
			if (minLen > (float)string2.Length)
				minLen = string2.Length;


			if (maxLen == 0.0F)
				return 1.0F;
			else
				return maxLen - dis;
		}

		static void normalize_Casing(ref string input)
		{
			//if it is formed by Pascal/Carmel casing
			for (int i = 0; i < input.Length; i++)
			{
				if (Char.IsSeparator(input[i]))
					input = input.Replace(input[i].ToString(), " ");
			}
			int idx = 1;
			while (idx < input.Length - 2)
			{
				++idx;
				if (
					(Char.IsUpper(input[idx])
					&& Char.IsLower(input[idx + 1]))
					&&
					(!Char.IsWhiteSpace(input[idx - 1]) && !Char.IsSeparator(input[idx - 1]))
					)
				{
					input = input.Insert(idx, " ");
					++idx;
				}
			}
		}

		static string[] partition(string input)
		{
			Regex r = new Regex("([ \\t{}():;.,\"'])");
			//Regex r = new Regex("([ \\t{}():;])");

			normalize_Casing(ref input);
			input = input.ToLower();

			String[] tokens = r.Split(input);

			List<string> filter = new List<string>();

			for (int i = 0; i < tokens.Length; i++)
			{
				MatchCollection mc = r.Matches(tokens[i]);
				if (mc.Count <= 0 && tokens[i].Trim().Length > 0)
					filter.Add(tokens[i]);

			}

			//tokens = new string[filter.Count];
			//for (int i = 0; i < filter.Count; i++) tokens[i] = (string)filter[i];

			//return tokens;
			return filter.ToArray();
		}
	}

	internal class BipartiteMatcher
	{
		private string[] _leftTokens, _rightTokens;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1814:PreferJaggedArraysOverMultidimensional", MessageId = "Member")]
		private readonly float[,] _cost;
		private float[] leftLabel, rightLabel;
		private int[] _previous, _incomming, _outgoing; //connect with the left and right

		private bool[] _leftVisited, _rightVisited;
		int leftLen, rightLen;
		readonly bool _errorOccured = false;

		public BipartiteMatcher(string[] left, string[] right, float[,] cost)
		{
			if (left == null || right == null || cost == null)
			{
				_errorOccured = true;
				return;
			}

			_leftTokens = left;
			_rightTokens = right;
			//swap
			if (_leftTokens.Length > _rightTokens.Length)
			{
				float[,] tmpCost = new float[_rightTokens.Length, _leftTokens.Length];
				for (int i = 0; i < _rightTokens.Length; i++)
					for (int j = 0; j < _leftTokens.Length; j++)
						tmpCost[i, j] = cost[j, i];

				_cost = (float[,])tmpCost.Clone();

				string[] tmp = _leftTokens;
				_leftTokens = _rightTokens;
				_rightTokens = tmp;
			}
			else
				_cost = (float[,])cost.Clone();


			MyInit();

			Make_Matching();
		}

		private void MyInit()
		{
			Initialize();

			_leftVisited = new bool[leftLen + 1];
			_rightVisited = new bool[rightLen + 1];
			_previous = new int[(leftLen + rightLen) + 2];

		}

		private void Initialize()
		{
			leftLen = _leftTokens.Length - 1;
			rightLen = _rightTokens.Length - 1;

			leftLabel = new float[leftLen + 1];
			rightLabel = new float[rightLen + 1];
			for (int i = 0; i < leftLabel.Length; i++) leftLabel[i] = 0;
			for (int i = 0; i < rightLabel.Length; i++) rightLabel[i] = 0;

			//init distance
			for (int i = 0; i <= leftLen; i++)
			{
				float maxLeft = float.MinValue;
				for (int j = 0; j <= rightLen; j++)
				{
					if (_cost[i, j] > maxLeft) maxLeft = _cost[i, j];
				}

				leftLabel[i] = maxLeft;
			}

		}

		private void Flush()
		{
			for (int i = 0; i < _previous.Length; i++) _previous[i] = -1;
			for (int i = 0; i < _leftVisited.Length; i++) _leftVisited[i] = false;
			for (int i = 0; i < _rightVisited.Length; i++) _rightVisited[i] = false;
		}

		bool stop = false;
		bool FindPath(int source)
		{
			Flush();
			stop = false;
			Walk(source);
			return stop;

		}

		void Increase_Matchs(int li, int lj)
		{
			int[] tmpOut = (int[])_outgoing.Clone();
			int i, j, k;
			i = li; j = lj;
			_outgoing[i] = j; _incomming[j] = i;
			if (_previous[i] != -1)
			{
				do
				{
					j = tmpOut[i];
					k = _previous[i];
					_outgoing[k] = j; _incomming[j] = k;
					i = k;
				} while (_previous[i] != -1);
			}
		}


		private void Walk(int i)
		{
			_leftVisited[i] = true;

			for (int j = 0; j <= rightLen; j++)
				if (stop) return;
				else
					if (!_rightVisited[j] && (leftLabel[i] + rightLabel[j] == _cost[i, j]))
				{
					if (_incomming[j] == -1)// if found a path
					{
						stop = true;
						Increase_Matchs(i, j);
						return;
					}
					else
					{
						int k = _incomming[j];
						_rightVisited[j] = true;
						_previous[k] = i;
						Walk(k);
					}
				}
		}

		#region BreadFirst
		//		int FindPath(int source)
		//		{
		//			int head, tail, idxHead=0;
		//			int[] visited=new int[(leftLen+rightLen)+2] , 
		//				q=new int[(leftLen+rightLen)+2] ;
		//			head=0;
		//			for (int i=0; i < visited.Length; i++) visited[i]=0;
		//			Flush ();
		//								
		//			head=-1;
		//			tail=0;
		//			q[tail]=source;
		//			visited[source]=1;
		//			leftVisited[source]=true;
		//			int nMerge=leftLen + rightLen + 1;
		//
		//			while (head <= tail)
		//			{
		//				++head;
		//				idxHead=q[head];
		//
		//
		//				for (int j=0; j <= (leftLen + rightLen + 1); j++)
		//					if(visited[j] == 0)
		//				{
		//					if (j > leftLen) //j is stay at the RightSide
		//					{
		//						int idxRight=j - (leftLen + 1);
		//						if (idxHead <= leftLen &&  (leftLabel[idxHead] + rightLabel[idxRight] == cost[idxHead, idxRight]))
		//						{
		//							++tail;
		//							q[tail]=j;
		//							visited[j]=1;
		//							previous[j]=idxHead;
		//							rightVisited[idxRight]=true;
		//							if (In[idxRight] == -1) // pretty good, found a path															
		//								return j;		
		//							
		//						}
		//					}
		//					else if ( j <= leftLen) // is stay at the left
		//					{
		//						if (idxHead > leftLen && In[idxHead - (leftLen + 1)] == j)
		//						{
		//							++tail;
		//							q[tail]=j;
		//							visited[j]=1;
		//							previous[j]=idxHead;
		//							leftVisited[j]=true;
		//						}
		//					}
		//				}
		//			}
		//
		//			return -1;//not found
		//		}
		//
		//		void Increase_Matchs(int j)
		//		{			
		//			if (previous [j] != -1)
		//				do
		//				{
		//					int i=previous[j];
		//					Out[i]=j-(leftLen + 1);
		//					In[j-(leftLen + 1)]=i;
		//					j=previous[i];
		//				} while ( j != -1);
		//		}
		//

		#endregion

		float GetMinDeviation()
		{
			float min = float.MaxValue;

			for (int i = 0; i <= leftLen; i++)
				if (_leftVisited[i])
				{
					for (int j = 0; j <= rightLen; j++)
						if (!_rightVisited[j])
						{
							if (leftLabel[i] + rightLabel[j] - _cost[i, j] < min)
								min = (leftLabel[i] + rightLabel[j]) - _cost[i, j];
						}
				}

			return min;
		}

		private void Relabels()
		{
			float dev = GetMinDeviation();

			for (int k = 0; k <= leftLen; k++)
				if (_leftVisited[k])
				{
					leftLabel[k] -= dev;
				}

			for (int k = 0; k <= rightLen; k++)
				if (_rightVisited[k])
				{
					rightLabel[k] += dev;
				}
		}

		private void Make_Matching()
		{
			_outgoing = new int[leftLen + 1];
			_incomming = new int[rightLen + 1];
			for (int i = 0; i < _outgoing.Length; i++) _outgoing[i] = -1;
			for (int i = 0; i < _incomming.Length; i++) _incomming[i] = -1;

			for (int k = 0; k <= leftLen; k++)
				if (_outgoing[k] == -1)
				{
					bool found = false;
					do
					{
						found = FindPath(k);
						if (!found) Relabels();

					} while (!found);
				}
		}


		private float GetTotal()
		{
			float nTotal = 0;
			float nA = 0;
			for (int i = 0; i <= leftLen; i++)
				if (_outgoing[i] != -1)
				{
					nTotal += _cost[i, _outgoing[i]];
					float a = 1.0F - System.Math.Max(_leftTokens[i].Length, _rightTokens[_outgoing[i]].Length) != 0 ? _cost[i, _outgoing[i]] / System.Math.Max(_leftTokens[i].Length, _rightTokens[_outgoing[i]].Length) : 1;
					nA += a;
				}
			return nTotal;
		}

		//public float GetScore()
		float getScore()
		{
			float dis = GetTotal();

			float maxLen = rightLen + 1;
			int l1 = 0; int l2 = 0;
			foreach (string s in _rightTokens) l1 += s.Length;
			foreach (string s in _leftTokens) l2 += s.Length;
			maxLen = Math.Max(l1, l2);

			if (maxLen > 0)
				return dis / maxLen;
			else
				return 1.0F;
		}


		public float Score
		{
			get
			{
				if (_errorOccured) return 0;
				else
					//return GetScore();
					return getScore();
			}
		}

	}
}
