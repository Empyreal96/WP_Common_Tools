using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace Microsoft.CompPlat.PkgBldr.Plugins.PkgToWm
{
	public class BooleanExpressionEvaluator
	{
		private string m_and;

		private string m_or;

		private string m_not;

		private SortedDictionary<string, bool> m_variables = new SortedDictionary<string, bool>();

		public string and
		{
			get
			{
				return m_and;
			}
			set
			{
				m_and = value;
				if (m_and != null)
				{
					m_and = m_and.ToLowerInvariant();
				}
			}
		}

		public string or
		{
			get
			{
				return m_or;
			}
			set
			{
				m_or = value;
				if (m_or != null)
				{
					m_or = m_or.ToLowerInvariant();
				}
			}
		}

		public string not
		{
			get
			{
				return m_not;
			}
			set
			{
				m_not = value;
				if (m_not != null)
				{
					m_not = m_not.ToLowerInvariant();
				}
			}
		}

		public string variablePattern { get; set; }

		public string expressionPattern { get; set; }

		public BooleanExpressionEvaluator()
		{
			and = "and";
			or = "or";
			not = "not";
			variablePattern = "[a-zA-Z][a-zA-Z0-9_\\-]+";
			expressionPattern = "^(.+)$";
		}

		private void setVariables(string expression)
		{
			foreach (Match item in new Regex(variablePattern).Matches(expression))
			{
				string text = item.Value.ToLowerInvariant();
				if (text == and || text == or || text == not)
				{
					continue;
				}
				switch (text)
				{
				case "(":
				case ")":
				case "=":
					continue;
				}
				if (!m_variables.ContainsKey(text))
				{
					m_variables.Add(text, false);
				}
			}
		}

		public string Evaluate(string expression)
		{
			if (expression == null)
			{
				throw new ArgumentNullException("expression");
			}
			if (!new Regex(expressionPattern).Match(expression).Success)
			{
				return null;
			}
			string text = expression.ToLowerInvariant();
			setVariables(text);
			if (and != null)
			{
				text = text.Replace(and, " and ");
			}
			if (or != null)
			{
				text = text.Replace(or, " or ");
			}
			if (not != null)
			{
				text = text.Replace(not, " not ");
			}
			foreach (KeyValuePair<string, bool> item in m_variables.Reverse())
			{
				text = Regex.Replace(text, item.Key ?? "", item.Value.ToString());
			}
			DataTable dataTable = new DataTable();
			dataTable.Locale = CultureInfo.InvariantCulture;
			dataTable.Columns.Add("", typeof(bool));
			dataTable.Columns[0].Expression = text;
			DataRow dataRow = dataTable.NewRow();
			dataTable.Rows.Add(dataRow);
			if ((bool)dataRow[0])
			{
				return "true";
			}
			return "false";
		}

		public void Set(string var, bool state)
		{
			if (var == null)
			{
				throw new ArgumentNullException("var");
			}
			var = var.ToLowerInvariant();
			if (!m_variables.ContainsKey(var))
			{
				m_variables.Add(var, state);
			}
			else
			{
				m_variables[var] = state;
			}
		}
	}
}
