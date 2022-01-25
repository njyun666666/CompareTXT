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
			Errors = new List<string>();
		}
		public string FileName { get; set; }
		public string OldPath { get; set; }
		public string NewPath { get; set; }
		public int OldLength { get; set; }
		public int NewLength { get; set; }
		public List<string> Errors { get; set; }
		public string Result
		{
			get
			{
				List<string> errors = Errors;

				if (string.IsNullOrWhiteSpace(NewPath))
				{
					errors.Add("缺少檔案");
				}

				if (OldLength != NewLength)
				{
					errors.Add($"行數不符, 舊行數: {OldLength}, 新行數: {NewLength}");
				}

				return $"{FileName},\"{string.Join("\n", errors)}\"\n";
			}
		}
	}
}
