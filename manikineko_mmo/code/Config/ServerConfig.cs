using Sandbox;
using Sandbox.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;

namespace Manikineko.MMO.Core.Config
{
	public class ServerConfig
	{
		public string name = "";
		public string server_homepage = "";
		public string description = "";
		public string discord_oauth = "" ;
		public long discord_server_id = -1;
		public bool discord_widget_enabled = false;
		public static string configversion = "1.0.0";
		public Logger logger = new Logger( "ServerConfig" );
		public static string serverconfig_path = "/cfg/server.json";

		public static ServerConfig GetServerConfig()
		{
			
			
			JsonSerializerOptions options = new JsonSerializerOptions(); ;
			options.IncludeFields = true;
			options.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull; 
			Logger serverconfig_logger = new Logger( "ServerConfig" );
			if ( FileSystem.Data.FileExists( serverconfig_path ) )
			{
				ServerConfig serverConfig;

				serverConfig = JsonSerializer.Deserialize<ServerConfig>( FileSystem.Data.ReadAllText( serverconfig_path ), options );
				serverconfig_logger.Info( "Server Config loaded" );
				if ( serverConfig != null )
				{
					return serverConfig;
				}

				else
				{
					serverconfig_logger.Error( "unable to load Server Config" );
					return defaultsrv();
				}
			}
			else
			{


				FileSystem.Data.CreateDirectory( "cfg" );
				
				string json = JsonSerializer.Serialize<ServerConfig>( defaultsrv(),options );


				

				serverconfig_logger.Info( json );
				serverconfig_logger.Info( "Server Config saved" );
				if ( json != "{}" )
				{
					FileSystem.Data.WriteAllText( serverconfig_path, json );
					return JsonSerializer.Deserialize<ServerConfig>( json,options );
				}

				else
				{
					serverconfig_logger.Error( "Default Server Config is empty!" );
					return defaultsrv();
				}
			}
		}
		public ServerConfig( string? name, string? server_homepage, string? description, string? discord_oauth, long? discord_server_id, bool? discord_widget_enabled,string? configversion )
		{
			if ( name == null )
			{
				this.name = "My Server";
			}
			else
			{
				this.name = name;
			}
			if ( server_homepage == null )
			{
				this.server_homepage = "https://www.example.com";
			}
			else
			{
				this.server_homepage = server_homepage;
			}
			if(description == null ) 
			{
				this.description= " ";
			}
			else
			{
				this.description = description;
			}
			if(discord_oauth == null )
			{
				this.discord_oauth = "https://discord.com";
			}
			else
			{
				this.discord_oauth =discord_oauth;
			}
			if(discord_server_id == null)
			{
				this.discord_server_id = -1;
			}
			else
			{
				this.discord_server_id = (long)discord_server_id;
			}
			if ( discord_widget_enabled != null && discord_widget_enabled != false)
			{
				
			}
			if(configversion != configversion )
			{
				logger.Error( "[Ciritcal Error] Config version invalid! Throwing error" );
				logger.Error( "also deleting the config data!" );

				throw new ConfigException( "Invalid Server Config: Version mismatch reboot server" );
			}
			
		}
		public ServerConfig()
		{

		}
		public static ServerConfig defaultsrv()
		{
			return new ServerConfig( null, null, null,null, null, null, configversion );
			
		}
	}
}
