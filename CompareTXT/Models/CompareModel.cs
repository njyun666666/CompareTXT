using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompareTXT.Models
{
	class CompareModel
	{
		public CompareModel()
		{
			RowErrors = new List<RowErrorModel>();
			Errors = new List<string>();
		}
		public string FileName { get; set; }
		public string OldPath { get; set; }
		public string NewPath { get; set; }
		public int OldLength { get; set; }
		public int NewLength { get; set; }
		public List<RowErrorModel> RowErrors { get; set; }
		public List<string> Errors { get; set; }
		public string Result
		{
			get
			{
				List<string> errors = Errors;
				StringBuilder rowErrorTable = new StringBuilder();

				if (string.IsNullOrWhiteSpace(NewPath))
				{
					errors.Add("缺少檔案");
				}

				if (OldLength != NewLength)
				{
					errors.Add($"行數不符, 舊行數: {OldLength}, 新行數: {NewLength}");
				}

				if (RowErrors.Any())
				{
					rowErrorTable.Append("<table>");
					rowErrorTable.Append($"<tr><td>行號</td><td></td><td>內容</td></tr>");

					foreach (var row in RowErrors)
					{
						rowErrorTable.Append($"<tr><td rowspan='3'>{row.RowNumber}</td><td>舊</td><td>{row.OldContent.Replace(" ", "&nbsp;")}</td></tr>");
						rowErrorTable.Append($"<tr><td>新</td><td>{row.NewContent.Replace(" ", "&nbsp;")}</td></tr>");
						rowErrorTable.Append($"<tr><td>差異</td><td>{row.Compare}</td></tr>");
					}

					rowErrorTable.Append("</table>");
				}

				return $"<tr><td>{FileName}</td><td>{string.Join("<br>", errors)}<br>{rowErrorTable}</td></tr>";
			}
		}
	}

	class RowErrorModel
	{
		public int RowNumber { get; set; }
		public string OldContent { get; set; }
		public string NewContent { get; set; }
		public string Compare { get; set; }
	}

}
