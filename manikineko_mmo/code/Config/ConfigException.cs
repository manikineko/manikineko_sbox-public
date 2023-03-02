using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manikineko.MMO.Core.Config
{
	public class ConfigException : Exception
	{
		public ConfigException(string message) : base(message)
		{

		}
	}
}
