
using System;

namespace com.jiuhuan.plan.domain {

    public class Log {

		public string owner { get; set; }
		public string level { get; set; }
		public string content { get; set; }
		public string ip { get; set; }
		public DateTime date { get; set; }
		public string note { get; set; } = "";
	}
}
