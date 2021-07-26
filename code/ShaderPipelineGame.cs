
using Sandbox;
using Sandbox.UI.Construct;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ShaderPipeline
{
	[Library( "shader_pipeline" )]
	public partial class ShaderPipeline : Sandbox.Game
	{
		public ShaderPipeline()
		{
		}

		public override void ClientJoined( Client client )
		{
			base.ClientJoined( client );

			var player = new ShaderPipelinePlayer();
			client.Pawn = player;
			player.Respawn();
		}
	}

}
