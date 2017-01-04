using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Specialized;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Security.Permissions;
using System.Web;
using System.Web.UI;

namespace Paraesthesia.Web.UI
{
	/// <summary>
	/// Handler factory for allowing ASP.NET applications to embed web forms as resources.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This page handler factory replaces the standard ASP.NET page handler
	/// factory and allows ASP.NET web forms to be served either from the
	/// filesystem (as usual) or from embedded resources in an assembly.
	/// </para>
	/// <para>
	/// Embedded web forms are enumerated from a configured set of assemblies
	/// at application startup and extracted to a temporary location.  When a
	/// request comes in, the filesystem is first examined to see if the page
	/// exists.  If it does, the user sees that page.  If it doesn't, the
	/// temporary location is investigated for the page.  If the page doesn't
	/// exist in either place, the user receives an error as usual.
	/// </para>
	/// <para>
	/// The <c>web.config</c> file must be configured with the list of assemblies
	/// containing embedded pages and the respective base namespace of each
	/// assembly.  This allows the embedded page handler factory to map an
	/// embedded resource name to a temporary filesystem path by removing the
	/// base namespace and replacing periods with directory separator characters.
	/// The drawback to this approach is that embedded pages are limited to
	/// not allow periods in the name and must end in the extension <c>.aspx</c>.
	/// </para>
	/// <para>
	/// As all pages get extracted to the same temporary location, embedded pages
	/// must have unique names/locations within the assemblies or unexpected
	/// results may occur.  For example, if two different assemblies each have
	/// a page called <c>Embedded.aspx</c> at the root level in the assembly,
	/// there will be a conflict and the first assembly found with the page
	/// will win out.  This being the case, it is recommended that a unique folder
	/// be created to contain all embedded pages for each assembly to avoid
	/// naming conflicts.
	/// </para>
	/// </remarks>
	/// <example>
	/// <para>
	/// The following example shows a <c>web.config</c> file with an assembly
	/// configured for embedded page service.  Note the following items:
	/// </para>
	/// <list type="bullet">
	/// <item>
	/// <term>Configuration section definition</term>
	/// <description>
	/// The <c>configuration/configSections</c> element has a <c>section</c>
	/// defined called <c>embeddedPageAssemblies</c> that gets processed by
	/// <see cref="System.Configuration.DictionarySectionHandler"/>.
	/// </description>
	/// </item>
	/// <item>
	/// <term>Embedded page assembly configuration</term>
	/// <description>
	/// The <c>configuration</c> element has a new element called
	/// <c>embeddedPageAssemblies</c> that has dictionary elements in it.
	/// Each key is the name of the assembly that contains the embedded pages
	/// (minus the .dll extension) and each value is the root namespace in that
	/// assembly.  Many times these will be the same.
	/// </description>
	/// </item>
	/// <item>
	/// <term>Module registration</term>
	/// <description>
	/// The <c>configuration/system.web/httpModules</c> element has a registration
	/// for the page handler factory.  This allows the embedded pages to be
	/// enumerated on app startup.
	/// </description>
	/// </item>
	/// <item>
	/// <term>Handler registration</term>
	/// <description>
	/// The <c>configuration/system.web/httpHandlers</c> element has a registration
	/// for the page handler factory.  The <c>verb</c> and <c>path</c>
	/// values match the ones specified for the standard ASP.NET page handler
	/// factory, so all requests will go through this factory instead of the
	/// standard one.
	/// </description>
	/// </item>
	/// <item>
	/// <term>Filesystem page setting</term>
	/// <description>
	/// The <c>appSettings</c> section has an optional key called
	/// <c>Paraesthesia.Web.UI.EmbeddedPageHandlerFactory.AllowFileSystemPages</c>
	/// with a Boolean value indicating if pages should be allowed to be served
	/// from the standard application filesystem.  If there is a problem reading
	/// the value or if the value doesn't exist, files will only be served from
	/// embedded resources.
	/// </description>
	/// </item>
	/// </list>
	/// <code>
	/// &lt;?xml version="1.0" encoding="utf-8" ?&gt;
	/// &lt;configuration&gt;
	///   &lt;configSections&gt;
	///     &lt;section
	///       name="embeddedPageAssemblies"
	///       type="System.Configuration.DictionarySectionHandler, System, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b77a5c561934e089, Custom=null"
	///       /&gt;
	///   &lt;/configSections&gt;
	///   &lt;embeddedPageAssemblies&gt;
	///     &lt;add
	///       key="Paraesthesia.EmbeddedPageHandlerFactory.Demo"
	///       value="Paraesthesia.EmbeddedPageHandlerFactory.Demo" /&gt;
	///   &lt;/embeddedPageAssemblies&gt;
	///   &lt;system.web&gt;
	///     &lt;compilation
	///       defaultLanguage="c#"
	///       debug="true" /&gt;
	///     &lt;customErrors mode="RemoteOnly" /&gt;
	///     &lt;authentication mode="Windows" /&gt;
	///     &lt;authorization&gt;
	///       &lt;allow users="*" /&gt;
	///     &lt;/authorization&gt;
	///     &lt;trace enabled="true"
	///       requestLimit="10"
	///       pageOutput="true"
	///       traceMode="SortByTime"
	///       localOnly="true" /&gt;
	///     &lt;sessionState
	///       mode="InProc"
	///       cookieless="false"
	///       timeout="20" /&gt;
	///     &lt;globalization
	///       requestEncoding="utf-8"
	///       responseEncoding="utf-8" /&gt;
	///     &lt;httpModules&gt;
	///       &lt;add
	///         name="EmbeddedPageHandlerFactory"
	///         type="Paraesthesia.Web.UI.EmbeddedPageHandlerFactory, Paraesthesia.EmbeddedPageHandlerFactory" /&gt;
	///     &lt;/httpModules&gt;
	///     &lt;httpHandlers&gt;
	///       &lt;add
	///         verb="*"
	///         path="*.aspx"
	///         type="Paraesthesia.Web.UI.EmbeddedPageHandlerFactory, Paraesthesia.EmbeddedPageHandlerFactory" /&gt;
	///     &lt;/httpHandlers&gt;
	///   &lt;/system.web&gt;
	///   &lt;/appSettings&gt;
	///     &lt;add
	///       key="Paraesthesia.Web.UI.EmbeddedPageHandlerFactory.AllowFileSystemPages"
	///       value="false" /&gt;
	///   &lt;appSettings&gt;
	/// &lt;/configuration&gt;
	/// </code>
	/// </example>
	[PermissionSet(SecurityAction.LinkDemand, XML="<PermissionSet class=\"System.Security.PermissionSet\" version=\"1\"><IPermission class=\"System.Security.Permissions.ReflectionPermission, mscorlib, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\" version=\"1\" Flags=\"TypeInformation, MemberAccess\"/></PermissionSet>")]
	public class EmbeddedPageHandlerFactory : IHttpHandlerFactory, IHttpModule
	{
		
		#region EmbeddedPageHandlerFactory Variables
        
		#region Constants
        
		/// <summary>
		/// Application settings ID for the value dictating whether pages are
		/// allowed in the filesystem.
		/// </summary>
		/// <seealso cref="Paraesthesia.Web.UI.EmbeddedPageHandlerFactory" />
		public const string ConfigIdAllowFileSystemPages = "Paraesthesia.Web.UI.EmbeddedPageHandlerFactory.AllowFileSystemPages";
		
		/// <summary>
		/// Name of the configuration section containing the set of assemblies
		/// to enumerate for embedded pages.
		/// </summary>
		/// <seealso cref="Paraesthesia.Web.UI.EmbeddedPageHandlerFactory" />
		public const string ConfigurationSectionName = "embeddedPageAssemblies";

		#endregion
        
		#region Statics
        
		// TODO: Refactor - add logging service and log statements throughout.

		/// <summary>
		/// Reference to internal <see langword="static" /> method called by
		/// built-in page parser factory.
		/// </summary>
		/// <seealso cref="Paraesthesia.Web.UI.EmbeddedPageHandlerFactory" />
		private static readonly MethodInfo PageParserGetCompiledPageInstance;

		/// <summary>
		/// Flag indicating whether the module has been initialized or not.
		/// </summary>
		/// <seealso cref="Paraesthesia.Web.UI.EmbeddedPageHandlerFactory" />
		private static bool ModuleInitialized = false;

		/// <summary>
		/// Object to synchronize locking on.
		/// </summary>
		/// <seealso cref="Paraesthesia.Web.UI.EmbeddedPageHandlerFactory" />
		private static readonly object SyncRoot;

		/// <summary>
		/// Collection of temporary files extracted to the filesystem.
		/// </summary>
		/// <seealso cref="Paraesthesia.Web.UI.EmbeddedPageHandlerFactory" />
		private static string TempFileBasePath = null;

		#endregion
        
		#endregion
        
        
        
		#region EmbeddedPageHandlerFactory Properties
        
		/// <summary>
		/// Gets a value indicating if pages can be served from the filesystem.
		/// </summary>
		/// <value>
		/// <see langword="true" /> if pages can be set from the filesystem;
		/// <see langword="false" /> if not.
		/// </value>
		/// <remarks>
		/// <para>
		/// This value corresponds to the <c>appSetting</c> value for
		/// <c>Paraesthesia.Web.UI.EmbeddedPageHandlerFactory.AllowFileSystemPages</c>.
		/// </para>
		/// </remarks>
		/// <seealso cref="Paraesthesia.Web.UI.EmbeddedPageHandlerFactory" />
		public virtual bool AllowFileSystemPages
		{
			get
			{
				bool retVal = false;
				string toParse = ConfigurationSettings.AppSettings[ConfigIdAllowFileSystemPages];
				if(toParse != null && toParse.Length > 0)
				{
					try
					{
						retVal = Convert.ToBoolean(toParse, CultureInfo.InvariantCulture);
					}
					catch
					{
						retVal = false;
					}
				}
				return retVal;
			}
		}

		#endregion
        
        
        
		#region EmbeddedPageHandlerFactory Implementation
        
		#region Constructors
        
		/// <summary>
		/// Initializes <see langword="static" /> properties of the <see cref="Paraesthesia.Web.UI.EmbeddedPageHandlerFactory" /> class.
		/// </summary>
		/// <seealso cref="Paraesthesia.Web.UI.EmbeddedPageHandlerFactory" />
		static EmbeddedPageHandlerFactory()
		{
			SyncRoot = new object();
			PageParserGetCompiledPageInstance = typeof(PageParser).GetMethod("GetCompiledPageInstanceInternal", BindingFlags.Static | BindingFlags.NonPublic);
		}

		#endregion
        
		#region Event Handlers
        
		/// <summary>
		/// Disposes of initialized handler items when the application is disposed.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">An <see cref="System.EventArgs" /> that contains the event data.</param>
		/// <seealso cref="Paraesthesia.Web.UI.EmbeddedPageHandlerFactory" />
		/// <seealso cref="Paraesthesia.Web.UI.EmbeddedPageHandlerFactory.Dispose" />
		private void ApplicationDisposed(object sender, EventArgs e)
		{
			this.Dispose();
		}

		#endregion
        
		#region Methods
        
		#region Static
        
		/// <summary>
		/// Removes temporary files generated by this factory.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Deletes the folder containing temporary files created by this
		/// factory and sets <see cref="Paraesthesia.Web.UI.EmbeddedPageHandlerFactory.TempFileBasePath"/>
		/// to <see langword="null" />.
		/// </para>
		/// <para>
		/// Call <see cref="Paraesthesia.Web.UI.EmbeddedPageHandlerFactory.InitializeTemporaryFileDirectory"/>
		/// to start a new temporary file extraction location.
		/// </para>
		/// </remarks>
		/// <seealso cref="Paraesthesia.Web.UI.EmbeddedPageHandlerFactory" />
		/// <seealso cref="Paraesthesia.Web.UI.EmbeddedPageHandlerFactory.InitializeTemporaryFileDirectory" />
		protected static void CleanupTemporaryFileDirectory()
		{
			if(TempFileBasePath != null)
			{
				if(Directory.Exists(TempFileBasePath))
				{
					Directory.Delete(TempFileBasePath, true);
				}
				TempFileBasePath = null;
			}
		}

		/// <summary>
		/// Initializes the temporary file extraction location.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Creates a temporary folder and sets the value at
		/// <see cref="Paraesthesia.Web.UI.EmbeddedPageHandlerFactory.TempFileBasePath"/>
		/// with the path to the location.
		/// </para>
		/// </remarks>
		/// <seealso cref="Paraesthesia.Web.UI.EmbeddedPageHandlerFactory" />
		/// <seealso cref="Paraesthesia.Web.UI.EmbeddedPageHandlerFactory.CleanupTemporaryFileDirectory" />
		protected static void InitializeTemporaryFileDirectory()
		{
			// Clean up existing files
			CleanupTemporaryFileDirectory();

			// Set up new temp file collection
			TempFileCollection tempFiles = new TempFileCollection();
			
			// Assert permissions for writing to a temporary file location.
			string tempFolderForPermission = Path.GetDirectoryName(tempFiles.BasePath);
			FileIOPermission tempPermission = new FileIOPermission(FileIOPermissionAccess.AllAccess, new string[]{tempFolderForPermission});
			tempPermission.Assert();

			// Create the temporary folder
			if(!Directory.Exists(tempFiles.BasePath))
			{
				Directory.CreateDirectory(tempFiles.BasePath);
			}
			TempFileBasePath = Path.GetFullPath(tempFiles.BasePath);
		}

		#endregion
        
		#region Instance
        
		/// <summary>
		/// Converts a path on the server's filesystem into a path in the temporary filesystem.
		/// </summary>
		/// <param name="path">The mapped path on the server.</param>
		/// <returns>
		/// A <see cref="System.String"/> with the path to the corresponding file
		/// in the temporary filesystem.
		/// </returns>
		/// <remarks>
		/// <para>
		/// If the <paramref name="path" /> does not exist inside the current
		/// <see cref="System.Web.HttpRuntime.AppDomainAppPath"/>, the
		/// <paramref name="path" /> is returned unchanged.  If it is in the
		/// current <see cref="System.Web.HttpRuntime.AppDomainAppPath"/>,
		/// the <see cref="System.Web.HttpRuntime.AppDomainAppPath"/> is
		/// removed from the <paramref name="path" /> and the remaining
		/// path parts are mapped into the temporary file location.
		/// </para>
		/// </remarks>
		/// <seealso cref="Paraesthesia.Web.UI.EmbeddedPageHandlerFactory" />
		protected virtual string ConvertServerPathToTempPath(string path)
		{
			// TODO: Refactor - use the argument validation service.
			if(path == null)
			{
				throw new ArgumentNullException("path", "Path to map may not be null.");
			}
			if(path.Length == 0)
			{
				throw new ArgumentOutOfRangeException("path", path, "Path to map may not be empty.");
			}

			// TODO: Refactor - use the UrlPath method to get the app path.
			string fullServerPath = Path.GetFullPath(path);
			if(String.Compare(fullServerPath, 0, HttpRuntime.AppDomainAppPath, 0, HttpRuntime.AppDomainAppPath.Length, true, CultureInfo.InvariantCulture) != 0)
			{
				return fullServerPath;
			}
			string subPath = path.Substring(HttpRuntime.AppDomainAppPath.Length).Replace("/", Path.DirectorySeparatorChar.ToString());
			if(subPath[0] == Path.DirectorySeparatorChar)
			{
				subPath = subPath.Substring(1);
			}
			string mappedPath = Path.Combine(TempFileBasePath, subPath);
			return mappedPath;

		}

		/// <summary>
		/// Extracts an embedded resource to a file in the filesystem.
		/// </summary>
		/// <param name="assembly">The <see cref="System.Reflection.Assembly"/> containing the resource.</param>
		/// <param name="resourcePath">The path to the embedded resource in the <paramref name="assembly" />.</param>
		/// <param name="destinationPath">The absolute path of the destination in the filesystem where the resource should be extracted.</param>
		/// <remarks>
		/// <para>
		/// If the folder that the resource needs to go into doesn't exist, the folder will
		/// be created.
		/// </para>
		/// </remarks>
		/// <exception cref="System.ArgumentNullException">
		/// Thrown if <paramref name="assembly" />, <paramref name="resourcePath" />, or
		/// <paramref name="destinationPath" /> is <see langword="null" />.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// Thrown if <paramref name="resourcePath" /> or <paramref name="destinationPath" />
		/// is <see cref="System.String.Empty"/>.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// Thrown if there is a problem writing the resource to a file or creating the
		/// destination directory (if applicable).  The <see cref="System.Exception.InnerException"/>
		/// will contain the reason the file wasn't written.
		/// </exception>
		/// <seealso cref="Paraesthesia.Web.UI.EmbeddedPageHandlerFactory" />
		protected virtual void ExtractResourceToFile(Assembly assembly, string resourcePath, string destinationPath)
		{
			// TODO: Refactor - use the argument validation service.
			// Validate parameters
			if(assembly == null)
			{
				throw new ArgumentNullException("assembly", "Assembly containing embedded resource may not be null.");
			}
			if(resourcePath == null)
			{
				throw new ArgumentNullException("resourcePath", "Path to embedded resource may not be null.");
			}
			if(resourcePath == String.Empty)
			{
				throw new ArgumentOutOfRangeException("resourcePath", "Path to embedded resource may not be empty.");
			}
			if(destinationPath == null)
			{
				throw new ArgumentNullException("destinationPath", "Destination path of embedded resource may not be null.");
			}
			if(destinationPath == String.Empty)
			{
				throw new ArgumentOutOfRangeException("destinationPath", "Destination path of embedded resource may not be empty.");
			}

			// Create the directory if it doesn't exist.
			try
			{
				string dirPath = Path.GetDirectoryName(destinationPath);
				if(!Directory.Exists(dirPath))
				{
					Directory.CreateDirectory(dirPath);
				}
			}
			catch(Exception err)
			{
				throw new System.IO.IOException(String.Format(CultureInfo.InvariantCulture, "Unable to create destination directory for path [{0}].", destinationPath), err);
			}

			// Extract the file
			Stream resStream = null;
			FileStream fstm = null;
			try
			{
				// Get the stream from the assembly resource.
				resStream = assembly.GetManifestResourceStream(resourcePath);

				// Get a filestream to write the data to.
				fstm = new FileStream(destinationPath, FileMode.CreateNew, FileAccess.Write, FileShare.ReadWrite);

				// Initialize properties for reading stream data
				long numBytesToRead = resStream.Length;
				int numBytesRead = 0;
				int bufferSize = 1024;
				byte[] bytes = new byte[bufferSize];

				// Read the file from the resource stream and write to the filesystem
				while(numBytesToRead > 0)
				{
					int numReadBytes = resStream.Read(bytes, 0, bufferSize);
					if(numReadBytes == 0)
					{
						break;
					}
					if(numReadBytes < bufferSize)
					{
						fstm.Write(bytes, 0, numReadBytes);
					}
					else
					{
						fstm.Write(bytes, 0, bufferSize);
					}
					numBytesRead += numReadBytes;
					numBytesToRead -= numReadBytes;
				}
				fstm.Flush();
			}
			catch(Exception err)
			{
				throw new System.IO.IOException(String.Format(CultureInfo.InvariantCulture, "Unable to write resource [{0}] from assembly [{1}] to destination [{2}].", resourcePath, assembly.FullName, destinationPath), err);
			}
			finally
			{
				// Close the resource stream
				if(resStream != null)
				{
					resStream.Close();
				}

				// Close the file
				if(fstm != null)
				{
					fstm.Close();
				}
			}
		}

		/// <summary>
		/// Retrieves the dictionary of settings for the module.
		/// </summary>
		/// <returns>
		/// An <see cref="System.Collections.IDictionary"/> where each key indicates
		/// the name of an assembly with embedded pages and the corresponding value
		/// is the root namespace of the assembly.
		/// </returns>
		/// <seealso cref="Paraesthesia.Web.UI.EmbeddedPageHandlerFactory" />
		protected virtual IDictionary GetConfiguredAssemblySection()
		{
			IDictionary settings = ConfigurationSettings.GetConfig(ConfigurationSectionName) as IDictionary;
			if(settings == null)
			{
				return new Hashtable();
			}
			return settings;
		}

		/// <summary>
		/// Gets a list of the embedded resource names that indicate web forms.
		/// </summary>
		/// <param name="assembly">The assembly to enumerate the embedded pages in.</param>
		/// <returns>
		/// An array of resource names indicating the embedded web forms in the
		/// provided assembly.
		/// </returns>
		/// <seealso cref="Paraesthesia.Web.UI.EmbeddedPageHandlerFactory" />
		/// <seealso cref="Paraesthesia.Web.UI.EmbeddedPageHandlerFactory.IsEmbeddedPageResource" />
		protected virtual string[] GetEmbeddedPageResourceNames(Assembly assembly)
		{
			// TODO: Refactor - use the argument validation service.
			if(assembly == null)
			{
				throw new ArgumentNullException("assembly", "Assembly to enumerate pages in may not be null.");
			}

			ArrayList pageList = new ArrayList();
			string[] resourceNames = assembly.GetManifestResourceNames();
			foreach(string resourceName in resourceNames)
			{
				if(this.IsEmbeddedPageResource(resourceName))
				{
					pageList.Add(resourceName);
				}
			}

			// Flatten array list to string array
			string[] retVal = (string[])pageList.ToArray(typeof(String));
			return retVal;
		}

		/// <summary>
		/// Determines if the provided resource name indicates an embedded web form.
		/// </summary>
		/// <param name="resourceName">The name of the embedded resource to check.</param>
		/// <returns>
		/// <see langword="true" /> if the resource ends with <c>.aspx</c>,
		/// <see langword="false" /> if not.
		/// </returns>
		/// <seealso cref="Paraesthesia.Web.UI.EmbeddedPageHandlerFactory" />
		protected bool IsEmbeddedPageResource(string resourceName)
		{
			// Resource must have at least one character for the filename and
			// five characters for the .aspx extension.
			if(resourceName == null || resourceName.Length < 6)
			{
				return false;
			}
			return String.Compare(".aspx", 0, resourceName, resourceName.Length - 5, 5, true, CultureInfo.InvariantCulture) == 0;
		}

		/// <summary>
		/// Maps an embedded resource ID into the filesystem.
		/// </summary>
		/// <param name="baseNamespace">
		/// The base namespace of the resource to map.
		/// </param>
		/// <param name="resourcePath">
		/// The fully qualified embedded resource path to map.
		/// </param>
		/// <param name="baseFolder">
		/// The base/root folder that the resource should be mapped into.  If this is
		/// <see langword="null" /> or <see cref="System.String.Empty" />, the current
		/// working directory is used.
		/// </param>
		/// <returns>The mapped path of the resource into the target folder.</returns>
		/// <remarks>
		/// <para>
		/// The <paramref name="baseNamespace" /> is stripped from the front of the
		/// <paramref name="resourcePath" /> and all but the last period in the remaining
		/// <paramref name="resourcePath" /> is replaced with the directory separator character
		/// (<see cref="System.IO.Path.DirectorySeparatorChar"/>).  Finally, that path
		/// is mapped into the <paramref name="baseFolder" />.
		/// </para>
		/// <para>
		/// The filename being mapped must have an extension associated with it, and that
		/// extension may not have a period in it.  Only one period will be kept in the
		/// mapped filename - others will be assumed to be directory separators.  If a filename
		/// has multiple extensions (i.e., <c>Custom.VAM.config</c>), it needs to be in
		/// the <paramref name="multipleExtensionFilenames" /> array.  The only exceptions
		/// are culture-specific .resx files -  If the resource is a <c>.resx</c> file (which will have to be named with a
		/// double-underscore - see below) and is culture-specific, the culture-specific
		/// information will be retained as long as the specific culture for which the
		/// file is meant can be created.  See below for examples.
		/// </para>
		/// <para>
		/// If <paramref name="baseNamespace" /> does not occur at the start of the
		/// <paramref name="resourcePath" />, an <see cref="System.InvalidOperationException"/>
		/// is thrown.
		/// </para>
		/// </remarks>
		/// <example>
		/// <para>
		/// Given a <paramref name="baseNamespace" /> of <c>MyNamespace</c> and a
		/// and a <paramref name="baseFolder" /> of <c>C:\temp</c>, this method will
		/// process <paramref name="resoucePath" /> as follows:
		/// </para>
		/// <list type="table">
		/// <listheader>
		/// <term><paramref name="resourcePath" /> value</term>
		/// <description>Mapping in Filesystem</description>
		/// </listheader>
		/// <item>
		/// <term><c>MyNamespace.Config.MyFile.config</c></term>
		/// <description><c>C:\temp\Config\MyFile.config</c></description>
		/// </item>
		/// </list>
		/// </example>
		/// <exception cref="System.ArgumentNullException">
		/// Thrown if <paramref name="baseNamespace" /> or <paramref name="resourcePath" /> is <see langword="null" />.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// Thrown if <paramref name="baseNamespace" /> or <paramref name="resourcePath" />:
		/// <list type="bullet">
		/// <item>
		/// <description>
		/// Is <see cref="System.String.Empty"/>.
		/// </description>
		/// </item>
		/// <item>
		/// <description>
		/// Start or end with period.
		/// </description>
		/// </item>
		/// <item>
		/// <description>
		/// Contain two or more periods together (like <c>MyNamespace..MySubnamespace</c>).
		/// </description>
		/// </item>
		/// </list>
		/// </exception>
		/// <seealso cref="Paraesthesia.Web.UI.EmbeddedPageHandlerFactory" />
		protected virtual string MapResourceToFileSystem(string baseNamespace, string resourcePath, string baseFolder)
		{
			// TODO: Refactor - use the argument validation service.
			// Validate parameters
			if(baseNamespace == null)
			{
				throw new ArgumentNullException("baseNamespace", "Base resource namespace may not be null.");
			}
			if(baseNamespace == String.Empty)
			{
				throw new ArgumentOutOfRangeException("baseNamespace", "Base resource namespace may not be empty.");
			}
			if(baseNamespace.StartsWith(".") || baseNamespace.EndsWith("."))
			{
				throw new ArgumentOutOfRangeException("baseNamespace", baseNamespace, "Base resource namespace may not start or end with a period.");
			}
			if(baseNamespace.IndexOf("..") >= 0)
			{
				throw new ArgumentOutOfRangeException("baseNamespace", baseNamespace, "Base resource namespace may not contain two or more periods together.");
			}
			if(resourcePath == null)
			{
				throw new ArgumentNullException("resourcePath", "Embedded resource path may not be null.");
			}
			if(resourcePath == String.Empty)
			{
				throw new ArgumentOutOfRangeException("resourcePath", "Embedded resource path may not be empty.");
			}
			if(resourcePath.StartsWith(".") || resourcePath.EndsWith("."))
			{
				throw new ArgumentOutOfRangeException("resourcePath", resourcePath, "Embedded resource path may not start or end with a period.");
			}
			if(resourcePath.IndexOf("..") >= 0)
			{
				throw new ArgumentOutOfRangeException("resourcePath", resourcePath, "Embedded resource path may not contain two or more periods together.");
			}

			// Ensure that the base namespace (with the period delimiter) appear in the resource path
			if(resourcePath.IndexOf(baseNamespace + ".") != 0)
			{
				throw new InvalidOperationException("Base resource namespace must appear at the start of the embedded resource path.");
			}

			// Get target folder
			string targetFolder = "";
			if(baseFolder == null || baseFolder == String.Empty)
			{
				targetFolder = Environment.CurrentDirectory;
			}
			else
			{
				targetFolder = Path.GetFullPath(baseFolder);
			}

			// Remove the base namespace from the resource path
			string newResourcePath = resourcePath.Remove(0, baseNamespace.Length + 1);

			// Find the last period - that's the file extension
			int extSeparator = newResourcePath.LastIndexOf(".");

			// Replace all but the last period with a directory separator
			string resourceFilePath = newResourcePath.Substring(0, extSeparator).Replace(".", Path.DirectorySeparatorChar.ToString(CultureInfo.InvariantCulture)) + newResourcePath.Substring(extSeparator, newResourcePath.Length - extSeparator);

			// Map the path into the target folder and return
			string retVal = Path.GetFullPath(Path.Combine(targetFolder, resourceFilePath));
			return retVal;
		}

		#endregion
        
		#endregion
        
		#endregion


		#region IHttpHandlerFactory Members

		/// <summary>
		/// Returns an instance of a class that implements the <see cref="System.Web.IHttpHandler"/> interface.
		/// </summary>
		/// <param name="context">An instance of the <see cref="System.Web.HttpContext"/> class that provides references to intrinsic server objects used to service HTTP requests.</param>
		/// <param name="requestType">The HTTP data transfer method (GET or POST) that the client uses.</param>
		/// <param name="url">The <see cref="System.Web.HttpRequest.RawUrl"/> of the requested resource.</param>
		/// <param name="pathTranslated">The <see cref="System.Web.HttpRequest.PhysicalApplicationPath"/> to the requested resource.</param>
		/// <returns>A new <see cref="System.Web.IHttpHandler"/> object that processes the request.</returns>
		/// <seealso cref="Paraesthesia.Web.UI.EmbeddedPageHandlerFactory" />
		[PermissionSet(SecurityAction.Assert, XML="<PermissionSet class=\"System.Security.PermissionSet\" version=\"1\"><IPermission class=\"System.Security.Permissions.SecurityPermission, mscorlib, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\" version=\"1\" Flags=\"UnmanagedCode\"/></PermissionSet>")]
		public virtual IHttpHandler GetHandler(HttpContext context, string requestType, string url, string pathTranslated)
		{
			// Ensure we're initialized when we get a request
			if(!ModuleInitialized)
			{
				System.Web.HttpApplication app = null;
				if(context != null)
				{
					app = context.ApplicationInstance;
				}
				this.Init(app);
			}

			// If the file exists in the filesystem, pass the request through
			if(this.AllowFileSystemPages && File.Exists(pathTranslated))
			{
				return PageParserGetCompiledPageInstance.Invoke(null, new object[]{url, pathTranslated, context}) as IHttpHandler;
			}

			// Map the path into the filesystem and return that
			// Call the public version of GetCompiledPageInstance so impersonation
			// gets handled correctly
			string mappedPath = this.ConvertServerPathToTempPath(pathTranslated);
			return PageParser.GetCompiledPageInstance(url, mappedPath, context);
		}

		/// <summary>
		/// Enables a factory to reuse an existing handler instance.
		/// </summary>
		/// <param name="handler">The <see cref="System.Web.IHttpHandler"/> object to reuse.</param>
		/// <remarks>
		/// <para>
		/// No action is taken; handlers are not pooled or explicitly reused.
		/// </para>
		/// </remarks>
		/// <seealso cref="Paraesthesia.Web.UI.EmbeddedPageHandlerFactory" />
		public virtual void ReleaseHandler(IHttpHandler handler)
		{
			// No action required.
		}

		#endregion


		#region IHttpModule Members

		/// <summary>
		/// Disposes of any resources used by the module.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Deletes temporary files extracted to the filesystem.
		/// </para>
		/// </remarks>
		/// <seealso cref="Paraesthesia.Web.UI.EmbeddedPageHandlerFactory" />
		/// <seealso cref="System.Web.IHttpModule.Dispose" />
		public virtual void Dispose()
		{
			try
			{
				// Clean up remaining temporary files
				CleanupTemporaryFileDirectory();
			}
			finally
			{
				// Set the factory to uninitialized
				ModuleInitialized = false;
			}
		}

		/// <summary>
		/// Initializes the embedded page handler factory module.
		/// </summary>
		/// <param name="context">
		/// The application for which the module is being initialized.
		/// </param>
		/// <remarks>
		/// <para>
		/// Initialization consists of enumerating the available set of embedded
		/// pages and setting up the temporary file extraction location.
		/// </para>
		/// <para>
		/// This module only gets initialized once per application.  If it needs to be
		/// reinitialized, the application needs to be restarted.
		/// </para>
		/// </remarks>
		/// <seealso cref="Paraesthesia.Web.UI.EmbeddedPageHandlerFactory" />
		/// <seealso cref="System.Web.IHttpModule.Init" />
		public virtual void Init(HttpApplication context)
		{
			// Don't initialize twice
			if(ModuleInitialized)
			{
				return;
			}

			lock(SyncRoot)
			{
				if(ModuleInitialized)
				{
					return;
				}

				// Attach to the application disposal event
				// This will get called when IIS resets or when the app pool is destroyed
				if(context != null)
				{
					context.Disposed += new EventHandler(this.ApplicationDisposed);
				}

				// Initialize extracted page lookup table and temporary location
				StringDictionary extractedPages = new StringDictionary();
				InitializeTemporaryFileDirectory();

				// Get the dictionary of assemblies and root namespaces
				IDictionary settings = this.GetConfiguredAssemblySection();

				// Enumerate through the set of assemblies and extract the pages
				foreach(string key in settings.Keys)
				{
					string asmName = key;
					string baseNamespace = (string)settings[key];
					Assembly asm = Assembly.Load(asmName);
					string[] resourceNames = this.GetEmbeddedPageResourceNames(asm);
					foreach(string resourceName in resourceNames)
					{
						string actualPath = this.MapResourceToFileSystem(baseNamespace, resourceName, TempFileBasePath);
						string virtualPath = actualPath.Substring(TempFileBasePath.Length);
						if(extractedPages.ContainsKey(virtualPath))
						{
							continue;
						}
						this.ExtractResourceToFile(asm, resourceName, actualPath);
						extractedPages.Add(virtualPath, actualPath);
					}
				}

				// Mark module as initialized
				ModuleInitialized = true;
			}
		}

		#endregion
	}
}
