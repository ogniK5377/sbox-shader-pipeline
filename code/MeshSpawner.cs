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
		private bool isViewport = false;

		public enum MeshType
		{
			Cube,
			Plane,
			Viewport,
			Sphere,
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
				_vb.AddCube( Vector3.Up * 32.0f, Vector3.One * 64.0f, Rotation.Identity );
			}
			else if ( meshType == MeshType.Plane )
			{
				Vertex[] vertex =
				{
					 new Vertex( new Vector3(-64.0f, 64.0f), Vector3.Up, Vector3.Right, new Vector2(0,0) ),
					 new Vertex( new Vector3(64.0f, 64.0f), Vector3.Up, Vector3.Right, new Vector2(8,0) ),
					 new Vertex( new Vector3(64.0f, -64.0f), Vector3.Up, Vector3.Right, new Vector2(8,8) ),
					 new Vertex( new Vector3(-64.0f, -64.0f), Vector3.Up, Vector3.Right, new Vector2(0,8) ),
				};

				_vb.AddQuad( vertex[3], vertex[2], vertex[1], vertex[0] );
			}
			else if ( meshType == MeshType.Viewport )
			{
				_vb.AddQuad( new Rect( -1f, -1f, 2, 2 ) );
				RenderBounds = BBox.FromHeightAndRadius( 2048 * 8, 2048 * 8 );
				isViewport = true;
			}
			else if ( meshType == MeshType.Sphere )
			{
				CreateSphere( 32.0f, 16, 16 );
			}
		}

		public Vector3 CalculateNormal( Vertex v1, Vertex v2, Vertex v3 )
		{
			Vector3 u = v2.Position - v1.Position;
			Vector3 v = v3.Position - v1.Position;

			return new Vector3
			{
				x = (u.y * v.z) - (u.z * v.y),
				y = (u.z * v.x) - (u.x * v.z),
				z = (u.x * v.y) - (u.y * v.x)
			};
		}

		public Vector4 CalculateTangent( Vertex v1, Vertex v2, Vertex v3 )
		{
			Vector4 u = v2.TexCoord0 - v1.TexCoord0;
			Vector4 v = v3.TexCoord0 - v1.TexCoord0;

			float r = 1.0f / (u.x * v.y - u.y * v.x);
			Vector3 tu = v2.Position - v1.Position;
			Vector3 vu = v3.Position - v1.Position;

			return new Vector4( (tu * v.y - vu * u.y) * r, 1.0f );
		}

		void CreatePlane( int widthSegments, int lengthSegments, float width, float length )
		{
			int hCount2 = widthSegments + 1;
			int vCount2 = lengthSegments + 1;
			int numTriangles = widthSegments * lengthSegments * 6;
			int numVertices = hCount2 * vCount2;
			Vector3[] tvertices = new Vector3[numVertices];
			Vector2[] tuvs = new Vector2[numVertices];
			int[] triangles = new int[numTriangles];

			int index = 0;
			float uvFactorX = 1.0f / widthSegments;
			float uvFactorY = 1.0f / lengthSegments;
			float scaleX = width / widthSegments;
			float scaleY = length / lengthSegments;

			for ( float y = 0.0f; y < vCount2; y++ )
			{
				for ( float x = 0.0f; x < hCount2; x++ )
				{
					tvertices[index] = new Vector3( x * scaleX - width / 2f, y * scaleY - length / 2f, 0.0f );
					tuvs[index++] = new Vector2( x * uvFactorX, y * uvFactorY );
				}
			}

			index = 0;
			for ( int y = 0; y < lengthSegments; y++ )
			{
				for ( int x = 0; x < widthSegments; x++ )
				{
					triangles[index] = (y * hCount2) + x;
					triangles[index + 1] = ((y + 1) * hCount2) + x;
					triangles[index + 2] = (y * hCount2) + x + 1;

					triangles[index + 3] = ((y + 1) * hCount2) + x;
					triangles[index + 4] = ((y + 1) * hCount2) + x + 1;
					triangles[index + 5] = (y * hCount2) + x + 1;
					index += 6;
				}
			}

			Vertex[] tvertex = new Vertex[numVertices];

			for ( int i = 0; i < numVertices; i++ )
			{
				tvertex[i] = new Vertex( Position + tvertices[i], Vector3.Up, Vector3.Right, tuvs[i] );
			}

			for ( int i = numTriangles - 1; i >= 0; i -= 3 )
			{
				int i1 = triangles[i];
				int i2 = triangles[i - 1];
				int i3 = triangles[i - 2];

				var v1 = tvertex[i1];
				var v2 = tvertex[i2];
				var v3 = tvertex[i3];

				Vector3 normal = CalculateNormal( v1, v2, v3 );
				Vector4 tangent = CalculateTangent( v1, v2, v3 );

				v1.Normal = normal;
				v2.Normal = normal;
				v3.Normal = normal;

				v1.Tangent = tangent;
				v2.Tangent = tangent;
				v3.Tangent = tangent;

				tvertex[i1] = v1;
				tvertex[i2] = v2;
				tvertex[i3] = v3;
			}

			for ( int i = 0; i < tvertex.Length; i++ )
			{
				_vb.Add( tvertex[i] );
			}


			for ( int i = numTriangles - 1; i >= 0; i-- )
			{
				_vb.AddRawIndex( triangles[i] );
			}
		}

		void CreateSphere( float radius, int sectorCount, int stackCount )
		{
			float lengthInv = 1.0f / radius;
			float sectorStep = 2.0f * MathF.PI / sectorCount;
			float stackStep = MathF.PI / stackCount;

			List<int> indicies = new();
			List<int> lineIndices = new();



			for ( int i = 0; i <= stackCount; i++ )
			{
				float stackAngle = MathF.PI / 2.0f - i * stackStep;

				float xy = radius * MathF.Cos( stackAngle );
				float z = radius * MathF.Sin( stackAngle );

				for ( int j = 0; j <= sectorCount; j++ )
				{
					float sectorAngle = j * sectorStep;
					float x = xy * MathF.Cos( sectorAngle );
					float y = xy * MathF.Sin( sectorAngle );

					Vector3 pos = new Vector3( x, y, z );
					Vector3 normal = pos * lengthInv;
					Vector4 uv = new Vector2( (float)j / (float)sectorCount, (float)i / (float)stackCount );

					Vector3 tangent1 = normal.Cross( Vector3.Forward );
					Vector3 tangent2 = normal.Cross( Vector3.Up );
					Vector3 tangent = tangent1.Length > tangent2.Length ? tangent1 : tangent2;

					_vb.Add( new Vertex( pos, normal, tangent, uv ) );
				}
			}

			for ( int i = 0; i < stackCount; i++ )
			{
				int k1 = i * (sectorCount + 1);
				int k2 = k1 + sectorCount + 1;
				for ( int j = 0; j < sectorCount; ++j, ++k1, ++k2 )
				{
					if ( i != 0 )
					{
						indicies.Add( k1 );
						indicies.Add( k2 );
						indicies.Add( k1 + 1 );

						/*
						indicies.Add( k1 + 1 );
						indicies.Add( k2 );
						indicies.Add( k1 );*/
					}

					if ( i != (stackCount - 1) )
					{
						indicies.Add( k1 + 1 );
						indicies.Add( k2 );
						indicies.Add( k2 + 1 );
						/*indicies.Add( k2 + 1 );
						indicies.Add( k2 );
						indicies.Add( k1 + 1 );*/
					}
				}
			}

			for ( int i = 0; i < indicies.Count; i++ )
			{
				_vb.AddRawIndex( indicies[i] );
			}
		}

		public override void DoRender( SceneObject obj )
		{
			if ( isViewport )
			{
				obj.Flags.IsOpaque = false;
				obj.Flags.IsTranslucent = false;
				obj.Flags.IsDecal = false;
				obj.Flags.OverlayLayer = false;
				obj.Flags.BloomLayer = false;
				obj.Flags.ViewModelLayer = false;
				obj.Flags.SkyBoxLayer = false;
				obj.Flags.NeedsLightProbe = false;
			}

			Render.CopyFrameBuffer();
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

		[ClientCmd( "shaderpl_sphere" )]
		public static void SpawnSphere()
		{
			TraceResult tr = Trace.Ray( Local.Pawn.EyePos, Local.Pawn.EyePos + Local.Pawn.EyeRot.Forward * 512 ).Ignore( Local.Pawn ).Run();
			Meshes.Add( new MeshSpawner( tr.EndPos + tr.Normal * 32.0f, MeshType.Sphere ) );
		}


		[ClientCmd( "shaderpl_viewport" )]
		public static void SpawnViewport()
		{
			Meshes.Add( new MeshSpawner( Vector3.Zero, MeshType.Viewport ) );
		}
	}
}
