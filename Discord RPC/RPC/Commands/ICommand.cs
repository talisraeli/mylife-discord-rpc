using RPC.RPC.Payload;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RPC.RPC.Commands
{
	interface ICommand
	{
		IPayload PreparePayload(long nonce);
	}
}
