using System;
using System.Globalization;

namespace SmartFoxClientAPI.Util
{
	public class MathUtil
	{
		public static bool IsNumeric(object expression)
		{
			if (expression == null || expression is DateTime)
			{
				return false;
			}
			if (expression is short || expression is int || expression is long || expression is decimal || expression is float || expression is double || expression is bool)
			{
				return true;
			}
			try
			{
				NumberFormatInfo numberFormat = new CultureInfo("en-US", useUserOverride: false).NumberFormat;
				if (expression is string)
				{
					double.Parse(expression as string, numberFormat);
				}
				else
				{
					double.Parse(expression.ToString(), numberFormat);
				}
				return true;
			}
			catch
			{
			}
			return false;
		}
	}
}
