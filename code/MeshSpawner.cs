using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShaderPipeline
{
	class MeshSpawner : RenderEntity
	{
		static private Material _material = Material.Load( "materials/example_shader_material.vmat" );
		private VertexBuffer _vb;

		public enum MeshType
		{
			Cube,
			Plane
		}

		public MeshSpawner( Vector3 position, MeshType meshType )
		{
			Position = position;
			_vb = new();
			_vb.Init( true );
			Transmit = TransmitType.Always;

			RenderBounds = BBox.FromHeightAndRadius( 128.0f, 128.0f );
			if ( meshType == MeshType.Cube )
			{
				_vb.AddCube( Position + Vector3.Up * 32.0f, Vector3.One * 64.0f, Rotation.Identity );
			}
			else
			{
				Vertex[] vertex =
				{
					 new Vertex( Position + new Vector3(-64.0f, 64.0f), Vector3.Up, Vector3.Right, new Vector2(0,0) ),
					 new Vertex( Position + new Vector3(64.0f, 64.0f), Vector3.Up, Vector3.Right, new Vector2(1,0) ),
					 new Vertex( Position + new Vector3(64.0f, -64.0f), Vector3.Up, Vector3.Right, new Vector2(1,1) ),
					 new Vertex( Position + new Vector3(-64.0f, -64.0f), Vector3.Up, Vector3.Right, new Vector2(0,1) ),
				};

				_vb.AddQuad( vertex[3], vertex[2], vertex[1], vertex[0] );
			}
		}

		public override void DoRender( SceneObject obj )
		{
			_vb.Draw( _material );
		}

		static List<MeshSpawner> Meshes = new();

		[ClientCmd( "shaderpl_cube" )]
		public static void SpawnCube()
		{
			TraceResult tr = Trace.Ray( Local.Pawn.EyePos, Local.Pawn.EyePos + Local.Pawn.EyeRot.Forward * 512 ).Ignore( Local.Pawn ).Run();
			Meshes.Add( new MeshSpawner( tr.EndPos + tr.Normal * 4.0f, MeshType.Cube ) );
		}

		[ClientCmd( "shaderpl_plane" )]
		public static void SpawnPlane()
		{
			TraceResult tr = Trace.Ray( Local.Pawn.EyePos, Local.Pawn.EyePos + Local.Pawn.EyeRot.Forward * 512 ).Ignore( Local.Pawn ).Run();
			Meshes.Add( new MeshSpawner( tr.EndPos + tr.Normal * 4.0f, MeshType.Plane ) );
		}
	}
}
