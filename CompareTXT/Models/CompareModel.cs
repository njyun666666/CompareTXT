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
	}

	class RowErrorModel
	{
		public int RowNumber { get; set; }
		public string OldContent { get; set; }
		public string NewContent { get; set; }
		public string Compare { get; set; }
	}

}
