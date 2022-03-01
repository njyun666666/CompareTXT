using CompareTXT.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
			Console.WriteLine($"start {startTime.ToString("yyyy-MM-dd HH:mm:ss")}");


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
			StringBuilder result = new StringBuilder();

			result.Append("<style>table{border-collapse: collapse;}th,td{border:1px solid}</style>");
			result.Append("<table><tr><th>檔名</th><th>比對結果</th></tr>");


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

						if (compareResult.RowErrors.Any())
						{
							item.RowErrors.AddRange(compareResult.RowErrors);
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
				result.Append($"<tr><td>{item}</td><td>多餘檔案</td></tr>");
			}


			result.Append("</table>");

			string savePath = $"result";
			if (!Directory.Exists(savePath))
			{
				Directory.CreateDirectory(savePath);
			}

			string fileSavePath = $@"{savePath}/compareTXT-{DateTime.Now.ToString("yyyyMMdd-HHmmss")}.html";
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

			string pattern = "\\r\\n|\\n|\\r";
			string[] textOld = Regex.Split(File.ReadAllText(fileOld), pattern, RegexOptions.IgnoreCase);
			string[] textNew = Regex.Split(File.ReadAllText(fileNew), pattern, RegexOptions.IgnoreCase);

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

						string Text2HtmlOut = CompuMaster.Text.Diffs.DumpDiffAsHtml(text, textNew[i], CompuMaster.Text.Diffs.EncodingRequirement.TextInputToBeEncodedIntoHtmlBeforeOutput);
						result.RowErrors.Add(new RowErrorModel()
						{
							RowNumber = i,
							OldContent = text,
							NewContent = textNew[i],
							Compare = Text2HtmlOut
						});
					}
				}

				if (errorRow.Count >= 10)
				{
					result.Errors.Add($"超過10行錯誤，停止比對");
					return result;
				}

				i++;
			}

			return result;
		}
	}
}