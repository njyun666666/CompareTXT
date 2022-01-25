using CompareTXT.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompareTXT
{
	class Program
	{
		static void Main(string[] args)
		{
			if (args.Length != 2)
			{
				Console.WriteLine($"路徑錯誤");
				return;
			}

			DateTime startTime = DateTime.Now;
			Console.WriteLine($"start {startTime}");
			

			string pathOld = args[0];
			string pathNew = args[1];

			if (!Directory.Exists(pathOld) || !Directory.Exists(pathNew))
			{
				Console.WriteLine($"路徑錯誤");
				return;
			}


			var filesOld = Directory.GetFiles(pathOld);
			var filesNew = Directory.GetFiles(pathNew);

			var targetList = (from o in filesOld
							  join n in filesNew
							  on Path.GetFileName(o).ToLower() equals Path.GetFileName(n).ToLower()
							  into leftjoin
							  from n in leftjoin.DefaultIfEmpty()
							  select new CompareModel
							  {
								  FileName = Path.GetFileName(o),
								  OldPath = o,
								  NewPath = n
							  }
							 ).ToList();

			List<Task> tasks = new List<Task>();
			StringBuilder result = new StringBuilder("檔名,比對結果\n");


			foreach (var item in targetList)
			{
				tasks.Add(Task.Run(() =>
				{
					if (!string.IsNullOrWhiteSpace(item.NewPath))
					{
						var compareResult = CompareFile(item.OldPath, item.NewPath);
						item.OldLength = compareResult.OldLength;
						item.NewLength = compareResult.NewLength;

						if (compareResult.Errors.Any())
						{
							item.Errors.AddRange(compareResult.Errors);
						}
					}

					Console.WriteLine($"{item.FileName} done");

				}));
			}

			Task.WhenAll(tasks).Wait();

			foreach (var item in targetList)
			{
				result.Append(item.Result);
			}



			var filesExcept = filesNew.Select(x => Path.GetFileName(x).ToLower()).Except(filesOld.Select(x => Path.GetFileName(x).ToLower()));

			foreach (var item in filesExcept)
			{
				result.Append($"{item},多餘檔案\n");
			}



			string savePath = $"result";
			if (!Directory.Exists(savePath))
			{
				Directory.CreateDirectory(savePath);
			}

			string fileSavePath = $@"{savePath}/compareTXT-{DateTime.Now.ToString("yyyyMMdd-HHmmss")}.csv";
			FileStream fs = new FileStream(fileSavePath, FileMode.CreateNew);
			
			using (StreamWriter sw = new StreamWriter(fs, Encoding.UTF8))
			{
				sw.Write(result);
			}


			DateTime endTime = DateTime.Now;
			Console.WriteLine($"end TotalSeconds: {(endTime - startTime).TotalSeconds}");




			//StringBuilder sb = new StringBuilder();
			//for (int i = 0; i < 500000; i++)
			//{
			//	sb.Append(i);
			//	sb.Append(" -----------------------------------------------------------------------------------------------------------------------------------------------------");
			//	sb.Append("\n");
			//}

			//for (int i = 0; i < 100; i++)
			//{
			//	string testFile = $@"C:\~project\txt\old\{i}.txt";

			//	using (StreamWriter sw = new StreamWriter(testFile))
			//	{
			//		sw.Write(sb);
			//	}
			//}
		}

		public static CompareModel CompareFile(string fileOld, string fileNew)
		{
			CompareModel result = new CompareModel();

			var textOld = File.ReadAllText(fileOld).Split("\n");
			var textNew = File.ReadAllText(fileNew).Split("\n");
			result.OldLength = textOld.Length;
			result.NewLength = textNew.Length;

			List<int> errorRow = new List<int>();

			int i = 0;
			foreach (var text in textOld)
			{
				if (i < result.NewLength)
				{
					if (text != textNew[i])
					{
						errorRow.Add(i + 1);
					}
				}

				if (errorRow.Count >= 10)
				{
					result.Errors.Add($"比對失敗行號: {string.Join(",", errorRow)} 超過10筆停止比對");
					return result;
				}

				i++;
			}

			if (errorRow.Any())
			{
				result.Errors.Add("比對失敗行號: " + string.Join(",", errorRow));
			}
			return result;
		}
	}
}