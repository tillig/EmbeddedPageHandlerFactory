using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Web;
using NUnit.Framework;
using TypeMock;
using EPHF = Paraesthesia.Web.UI.EmbeddedPageHandlerFactory;

namespace Paraesthesia.Web.UI.Test
{
	/// <summary>
	/// Test fixture for <see cref="Paraesthesia.Web.UI.EmbeddedPageHandlerFactory"/>.
	/// </summary>
	[TestFixture]
	public class EmbeddedPageHandlerFactory
	{
		#region Variables and Constants
        
 
		/// <summary>
		/// Path to an embedded resource used for testing.  Standard text file.
		/// </summary>
		private const string ResourcePathStandard = "Paraesthesia.Web.UI.Test.Resources.TextFile1.txt";
 
		/// <summary>
		/// Path to an embedded resource used for testing.  Zero-length file.
		/// </summary>
		private const string ResourcePathZeroLength = "Paraesthesia.Web.UI.Test.Resources.ZeroLength.txt";

		/// <summary>
		/// Path to an embedded resource used for testing.  Contents are longer than one buffer in the extraction logic.
		/// </summary>
		private const string ResourcePathLong = "Paraesthesia.Web.UI.Test.Resources.Long.txt";

		/// <summary>
		/// Path to an embedded resource used for testing.  Contents are exactly the length of one buffer in the extraction logic.
		/// </summary>
		private const string ResourcePathBufferLength = "Paraesthesia.Web.UI.Test.Resources.BufferLength.txt";

		/// <summary>
		/// Size in bytes of the embedded resource.
		/// </summary>
		private const int ResourceLengthStandard = 20;
 
		/// <summary>
		/// Size in bytes of the embedded resource.
		/// </summary>
		private const int ResourceLengthZeroLength = 0;

		/// <summary>
		/// Size in bytes of the embedded resource.
		/// </summary>
		private const int ResourceLengthLong = 1350;

		/// <summary>
		/// Size in bytes of the embedded resource.
		/// </summary>
		private const int ResourceLengthBufferLength = 1024;

		/// <summary>
		/// Page handler factory used in tests.
		/// </summary>
		private EPHF _factory = null;

		/// <summary>
		/// Mock page handler factory used for isolating calls in tests.
		/// </summary>
		private MockObject _mockFactory = null;

		/// <summary>
		/// Temporary file collection used in tests.
		/// </summary>
		private TempFileCollection _tempFiles = null;
        
		#endregion
        
        
		#region SetUp/TearDown
        
		[SetUp]
		public void SetUp()
		{
			if(!MockManager.IsInitialized)
			{
				MockManager.Init();
			}

			// Set up factory
			this._mockFactory = MockManager.MockObject(typeof(EPHF), Constructor.NotMocked);
			this._factory = this._mockFactory.Object as EPHF;

			// Set up temporary file location
			this._tempFiles = new TempFileCollection();
			this._tempFiles.KeepFiles = false;
			if(!Directory.Exists(this._tempFiles.BasePath))
			{
				Directory.CreateDirectory(this._tempFiles.BasePath);
			}
		}
        
		[TearDown]
		public void TearDown()
		{
			// Remove any temporary path created during tests
			this._factory.Dispose();

			// Clean up factory
			this._factory = null;
			this._mockFactory = null;

			// Remove temporary files
			if(this._tempFiles != null)
			{
				this._tempFiles.Delete();
				if(Directory.Exists(this._tempFiles.BasePath))
				{
					Directory.Delete(this._tempFiles.BasePath, true);
				}
				this._tempFiles = null;
			}

			MockManager.ClearAll();
		}
        
		#endregion
        
        
		#region Tests

		#region Constructor

		[Test(Description="Tests the static constructor initialization.")]
		public void StaticConstructor()
		{
			object syncRoot = this.GetNonPublicStaticFieldValue("SyncRoot");
			Assert.IsNotNull(syncRoot, "The synchronization root object should be initialized.");
			MethodInfo pageParserGetCompiledPageInstance = this.GetNonPublicStaticFieldValue("PageParserGetCompiledPageInstance") as MethodInfo;
			Assert.IsNotNull(pageParserGetCompiledPageInstance, "The non-public version of the compiled page instance retrieval method should be found.");
		}

		#endregion

        #region AllowFileSystemPages
		
		[Test(Description="Tests what happens when the AllowFileSystemPages setting is configured with an empty value.")]
		public void AllowFileSystemPages_EmptyConfig()
		{
			using (RecordExpectations recorder = RecorderManager.StartRecording())
			{
				string dummy = ConfigurationSettings.AppSettings[Paraesthesia.Web.UI.EmbeddedPageHandlerFactory.ConfigIdAllowFileSystemPages];
				recorder.Return("");
				recorder.RepeatAlways();
			}
			bool actual = this._factory.AllowFileSystemPages;
			Assert.IsFalse(actual, "If the property is configured empty, the default should be false.");
			MockManager.Verify();
		}
		
		[Test(Description="Tests what happens when the AllowFileSystemPages setting is configured with a non-Boolean value.")]
		public void AllowFileSystemPages_InvalidConfig()
		{
			using (RecordExpectations recorder = RecorderManager.StartRecording())
			{
				string dummy = ConfigurationSettings.AppSettings[Paraesthesia.Web.UI.EmbeddedPageHandlerFactory.ConfigIdAllowFileSystemPages];
				recorder.Return("foo");
				recorder.RepeatAlways();
			}
			bool actual = this._factory.AllowFileSystemPages;
			Assert.IsFalse(actual, "If the property is improperly configured, the default should be false.");
			MockManager.Verify();
		}
		
		[Test(Description="Tests what happens when the AllowFileSystemPages setting is not configured.")]
		public void AllowFileSystemPages_NoConfig()
		{
			using (RecordExpectations recorder = RecorderManager.StartRecording())
			{
				string dummy = ConfigurationSettings.AppSettings[Paraesthesia.Web.UI.EmbeddedPageHandlerFactory.ConfigIdAllowFileSystemPages];
				recorder.Return(null);
				recorder.RepeatAlways();
			}
			bool actual = this._factory.AllowFileSystemPages;
			Assert.IsFalse(actual, "If the property is not configured, the default should be false.");
			MockManager.Verify();
		}
		
		[Test(Description="Tests what happens when the AllowFileSystemPages setting is configured with a Boolean value.")]
		public void AllowFileSystemPages_ValidConfig()
		{
			using (RecordExpectations recorder = RecorderManager.StartRecording())
			{
				string dummy = ConfigurationSettings.AppSettings[Paraesthesia.Web.UI.EmbeddedPageHandlerFactory.ConfigIdAllowFileSystemPages];
				recorder.Return("true");
				recorder.RepeatAlways();
			}
			bool actual = this._factory.AllowFileSystemPages;
			Assert.IsTrue(actual, "If the property is properly configured, the configured value should be returned.");
			MockManager.Verify();
		}

		#endregion

		#region ApplicationDisposed

		[Test(Description="Verifies that Dispose is called when the ApplicationDisposed handler is called.")]
		public void ApplicationDisposed_DisposeCalled()
		{
			using(RecordExpectations recorder = RecorderManager.StartRecording())
			{
				this._factory.Dispose();
			}
			this.ApplicationDisposed();
			MockManager.Verify();
		}
		
		#endregion

		#region ConvertServerPathToTempPath

		[Test(Description="Exception should be thrown if the path to map is empty.")]
		[ExpectedException(typeof(System.ArgumentOutOfRangeException))]
		public void ConvertServerPathToTempPath_Arg_Empty()
		{
			this.ConvertServerPathToTempPath("");
		}

		[Test(Description="Exception should be thrown if the path to map is null.")]
		[ExpectedException(typeof(System.ArgumentNullException))]
		public void ConvertServerPathToTempPath_Arg_Null()
		{
			this.ConvertServerPathToTempPath(null);
		}

		[Test(Description="If the path is in the application, the mapped path should be returned.")]
		public void ConvertServerPathToTempPath_PathInApp()
		{
			using(RecordExpectations recorder = RecorderManager.StartRecording())
			{
				// TODO: Refactor - use the UrlPath method to get the app path.
				string dummy = HttpRuntime.AppDomainAppPath;
				recorder.RepeatAlways();
				recorder.Return(@"C:\actualapp");
			}
			this.InvokeNonPublicStaticMember("InitializeTemporaryFileDirectory", new object[]{});
			string expected = Path.Combine((string)this.GetNonPublicStaticFieldValue("TempFileBasePath"), @"folder\file.aspx");
			string actual = this.ConvertServerPathToTempPath(@"C:\actualapp\folder\file.aspx");
			Assert.AreEqual(expected, actual, "If the converted path is in the temp folder, it should be returned directly.");
			MockManager.Verify();
		}

		[Test(Description="If the path is outside the application, the original path should be returned.")]
		public void ConvertServerPathToTempPath_PathNotInApp()
		{
			using(RecordExpectations recorder = RecorderManager.StartRecording())
			{
				// TODO: Refactor - use the UrlPath method to get the app path.
				string dummy = HttpRuntime.AppDomainAppPath;
				recorder.RepeatAlways();
				recorder.Return(@"C:\actualapp");
			}
			this.InvokeNonPublicStaticMember("InitializeTemporaryFileDirectory", new object[]{});
			string expected = @"C:\MyApp\myfile.aspx";
			string actual = this.ConvertServerPathToTempPath(expected);
			Assert.AreEqual(expected, actual, "If the converted path is in the temp folder, it should be returned directly.");
			MockManager.Verify();
		}

		#endregion

		#region ExtractResourceToFile

		[Test(Description="Attempts to extract a resource from a null assembly.")]
		[ExpectedException(typeof(System.ArgumentNullException))]
		public void ExtractResourceToFile_Arg_Assembly()
		{
			this.ExtractResourceToFile(null, "resourcePath", "destinationPath");
		}

		[Test(Description="Attempts to extract a resource using a null destination path.")]
		[ExpectedException(typeof(System.ArgumentNullException))]
		public void ExtractResourceToFile_Arg_DestinationPath1()
		{
			this.ExtractResourceToFile(Assembly.GetExecutingAssembly(), "resourcePath", null);
		}

		[Test(Description="Attempts to extract a resource using an empty destination path.")]
		[ExpectedException(typeof(System.ArgumentOutOfRangeException))]
		public void ExtractResourceToFile_Arg_DestinationPath2()
		{
			this.ExtractResourceToFile(Assembly.GetExecutingAssembly(), "resourcePath", "");
		}

		[Test(Description="Attempts to extract a resource using a null resource path.")]
		[ExpectedException(typeof(System.ArgumentNullException))]
		public void ExtractResourceToFile_Arg_ResourcePath1()
		{
			this.ExtractResourceToFile(Assembly.GetExecutingAssembly(), null, "destinationPath");
		}

		[Test(Description="Attempts to extract a resource using an empty resource path.")]
		[ExpectedException(typeof(System.ArgumentOutOfRangeException))]
		public void ExtractResourceToFile_Arg_ResourcePath2()
		{
			this.ExtractResourceToFile(Assembly.GetExecutingAssembly(), "", "destinationPath");
		}

		[Test(Description="Attempts to extract a resource using a path to an embedded resource that doesn't exist.")]
		[ExpectedException(typeof(System.IO.IOException))]
		public void ExtractResourceToFile_InvalidResourcePath()
		{
			string destinationPath = Path.Combine(_tempFiles.BasePath, "testfile.txt");
			this.ExtractResourceToFile(Assembly.GetExecutingAssembly(), "non_existent_resource", destinationPath);
		}

		[Test(Description="Extracts a resource to file.")]
		public void ExtractResourceToFile_Valid()
		{
			string destinationPath = Path.Combine(_tempFiles.BasePath, "testfile.txt");
			this.ExtractResourceToFile(Assembly.GetExecutingAssembly(), ResourcePathStandard, destinationPath);
			Assert.IsTrue(File.Exists(destinationPath), String.Format("File [{0}] doesn't exist - file not correctly extracted.", destinationPath));
			_tempFiles.AddFile(destinationPath, false);
			FileInfo fileInfo = new FileInfo(destinationPath);
			Assert.AreEqual(ResourceLengthStandard, fileInfo.Length, String.Format("File [{0}] was not the correct length.", destinationPath));
		}

		[Test(Description="Extracts a buffer-length resource to file.")]
		public void ExtractResourceToFile_Valid_BufferLength()
		{
			string destinationPath = Path.Combine(_tempFiles.BasePath, "testfile.txt");
			this.ExtractResourceToFile(Assembly.GetExecutingAssembly(), ResourcePathBufferLength, destinationPath);
			Assert.IsTrue(File.Exists(destinationPath), String.Format("File [{0}] doesn't exist - file not correctly extracted.", destinationPath));
			_tempFiles.AddFile(destinationPath, false);
			FileInfo fileInfo = new FileInfo(destinationPath);
			Assert.AreEqual(ResourceLengthBufferLength, fileInfo.Length, String.Format("File [{0}] was not the correct length.", destinationPath));
		}

		[Test(Description="Extracts a resource to file where a path needs to be created.")]
		public void ExtractResourceToFile_Valid_CreateDirectory()
		{
			string destinationPath = Path.Combine(_tempFiles.BasePath, String.Format("testfolder{0}testfile.txt", Path.DirectorySeparatorChar));
			this.ExtractResourceToFile(Assembly.GetExecutingAssembly(), ResourcePathStandard, destinationPath);
			Assert.IsTrue(File.Exists(destinationPath), String.Format("File [{0}] doesn't exist - file not correctly extracted.", destinationPath));
			_tempFiles.AddFile(destinationPath, false);
			FileInfo fileInfo = new FileInfo(destinationPath);
			Assert.AreEqual(ResourceLengthStandard, fileInfo.Length, String.Format("File [{0}] was not the correct length.", destinationPath));
		}

		[Test(Description="Extracts a resource with a size larger than the read buffer to file.")]
		public void ExtractResourceToFile_Valid_Long()
		{
			string destinationPath = Path.Combine(_tempFiles.BasePath, "testfile.txt");
			this.ExtractResourceToFile(Assembly.GetExecutingAssembly(), ResourcePathLong, destinationPath);
			Assert.IsTrue(File.Exists(destinationPath), String.Format("File [{0}] doesn't exist - file not correctly extracted.", destinationPath));
			_tempFiles.AddFile(destinationPath, false);
			FileInfo fileInfo = new FileInfo(destinationPath);
			Assert.AreEqual(ResourceLengthLong, fileInfo.Length, String.Format("File [{0}] was not the correct length.", destinationPath));
		}

		[Test(Description="Extracts a zero-byte length resource to file.")]
		public void ExtractResourceToFile_Valid_ZeroLength()
		{
			string destinationPath = Path.Combine(_tempFiles.BasePath, "testfile.txt");
			this.ExtractResourceToFile(Assembly.GetExecutingAssembly(), ResourcePathZeroLength, destinationPath);
			Assert.IsTrue(File.Exists(destinationPath), String.Format("File [{0}] doesn't exist - file not correctly extracted.", destinationPath));
			_tempFiles.AddFile(destinationPath, false);
			FileInfo fileInfo = new FileInfo(destinationPath);
			Assert.AreEqual(ResourceLengthZeroLength, fileInfo.Length, String.Format("File [{0}] was not the correct length.", destinationPath));
		}

		#endregion

		#region GetConfiguredAssemblySection
		
		[Test(Description="Attempts to retrieve a section that isn't configured.")]
		public void GetConfiguredAssemblySection_Null()
		{
			using (RecordExpectations recorder = RecorderManager.StartRecording())
			{
				IDictionary dummy = ConfigurationSettings.GetConfig(EPHF.ConfigurationSectionName) as IDictionary;
				recorder.RepeatAlways();
				recorder.Return(null);
			}
			IDictionary actual = this.GetConfiguredAssemblySection();
			Assert.IsNotNull(actual, "The returned configuration should not be null if not configured.");
			Assert.IsInstanceOfType(typeof(Hashtable), actual, "The default return value should be a hashtable.");
			MockManager.Verify();
		}
		
		[Test(Description="Attempts to retrieve a section that isn't configured.")]
		public void GetConfiguredAssemblySection_Valid()
		{
			Hashtable expected = new Hashtable();
			using (RecordExpectations recorder = RecorderManager.StartRecording())
			{
				IDictionary dummy = ConfigurationSettings.GetConfig(EPHF.ConfigurationSectionName) as IDictionary;
				recorder.RepeatAlways();
				recorder.Return(expected);
			}
			IDictionary actual = this.GetConfiguredAssemblySection();
			Assert.IsNotNull(actual, "The returned configuration should not be null if configured.");
			Assert.AreEqual(expected, actual, "The expected configuration data was not returned.");
			MockManager.Verify();
		}

		#endregion

		#region GetEmbeddedPageResourceNames

		[Test(Description="Tests retrieving embedded page resources from an assembly that has none.")]
		public void GetEmbeddedPageResourceNames_None()
		{
			string[] actual = this.GetEmbeddedPageResourceNames(Assembly.GetAssembly(typeof(System.String)));
			Assert.IsNotNull(actual, "An assembly with no pages should not return a null collection.");
			Assert.AreEqual(0, actual.Length, "An assembly with no pages should return an empty collection.");
		}

		[Test(Description="Ensures you can't get resources from a null assembly.")]
		[ExpectedException(typeof(System.ArgumentNullException))]
		public void GetEmbeddedPageResourceNames_Null()
		{
			this.GetEmbeddedPageResourceNames(null);
		}

		[Test(Description="Tests retrieving embedded page resources from an assembly that has some.")]
		public void GetEmbeddedPageResourceNames_Valid()
		{
			string[] actual = this.GetEmbeddedPageResourceNames(Assembly.GetExecutingAssembly());
			Assert.IsNotNull(actual, "An assembly with pages should not return a null collection.");
			Assert.AreEqual(1, actual.Length, "An assembly with pages should not return an empty collection.");
		}

		#endregion

		#region Init

		[Test(Description="Calls Init when the module is already initialized.")]
		public void Init_Initialized()
		{
			this.ModuleInitialized = true;
			this._factory.Init(null);
		}

		[Test(Description="Tests Init being called with an empty configuration available.")]
		public void Init_EmptyConfig()
		{
			bool initialized = this.ModuleInitialized;
			Assert.IsFalse(initialized, "Module should not be initialized prior to its first Init call.");
			using(RecordExpectations recorder = RecorderManager.StartRecording())
			{
				object dummy = ConfigurationSettings.GetConfig(EPHF.ConfigurationSectionName);
				recorder.Return(new Hashtable());
			}
			this._factory.Init(null);
			initialized = this.ModuleInitialized;
			Assert.IsTrue(initialized, "Module should be initialized after a call to Init.");
			this.AssertPathContents(this.TempFileBasePath, 0, 0);
			MockManager.Verify();
		}

		[Test(Description="Tests Init being called with no configuration available.")]
		public void Init_NoConfig()
		{
			bool initialized = this.ModuleInitialized;
			Assert.IsFalse(initialized, "Module should not be initialized prior to its first Init call.");
			using(RecordExpectations recorder = RecorderManager.StartRecording())
			{
				object dummy = ConfigurationSettings.GetConfig(EPHF.ConfigurationSectionName);
				recorder.Return(null);
			}
			this._factory.Init(null);
			initialized = this.ModuleInitialized;
			Assert.IsTrue(initialized, "Module should be initialized after a call to Init.");
			this.AssertPathContents(this.TempFileBasePath, 0, 0);
			MockManager.Verify();
		}

		[Test(Description="Tests Init being called with config specifying an assembly that has no embedded resources.")]
		public void Init_NoResources()
		{
			bool initialized = this.ModuleInitialized;
			Assert.IsFalse(initialized, "Module should not be initialized prior to its first Init call.");
			
			// Mock settings containing the current assembly's name
			string asmName = Assembly.GetExecutingAssembly().GetName().Name;
			Hashtable settings = new Hashtable();
			settings[asmName] = typeof(EmbeddedPageHandlerFactory).Namespace;
			using(RecordExpectations recorder = RecorderManager.StartRecording())
			{
				object dummy = ConfigurationSettings.GetConfig(EPHF.ConfigurationSectionName);
				recorder.Return(settings);
			}

			// Mock that there are no resources
			this._mockFactory.ExpectAndReturn("GetEmbeddedPageResourceNames", new string[]{});

			this._factory.Init(null);
			initialized = this.ModuleInitialized;
			Assert.IsTrue(initialized, "Module should be initialized after a call to Init.");
			this.AssertPathContents(this.TempFileBasePath, 0, 0);
			MockManager.Verify();
		}

		[Test(Description="Tests Init being called with config specifying an assembly that has embedded resources.")]
		public void Init_Resources()
		{
			bool initialized = this.ModuleInitialized;
			Assert.IsFalse(initialized, "Module should not be initialized prior to its first Init call.");
			
			// Mock settings containing the current assembly's name
			string asmName = Assembly.GetExecutingAssembly().GetName().Name;
			Hashtable settings = new Hashtable();
			settings[asmName] = typeof(EmbeddedPageHandlerFactory).Namespace;
			using(RecordExpectations recorder = RecorderManager.StartRecording())
			{
				object dummy = ConfigurationSettings.GetConfig(EPHF.ConfigurationSectionName);
				recorder.Return(settings);
			}
			
			this._factory.Init(null);
			initialized = this.ModuleInitialized;
			Assert.IsTrue(initialized, "Module should be initialized after a call to Init.");
			this.AssertPathContents(this.TempFileBasePath, 1, 1);
			MockManager.Verify();
		}

		#endregion

		#region IsEmbeddedPageResource

		[Test(Description="Ensures an empty resource name is not considered an embedded page.")]
		public void IsEmbeddedPageResource_Empty()
		{
			bool actual = this.IsEmbeddedPageResource("");
			Assert.IsFalse(actual, "Empty resource name should not be a page.");
		}

		[Test(Description="Ensures a resource name with only .aspx in it is not considered an embedded page.")]
		public void IsEmbeddedPageResource_ExtensionOnly()
		{
			bool actual = this.IsEmbeddedPageResource(".aspx");
			Assert.IsFalse(actual, "Extension-only resource name should not be a page.");
		}

		[Test(Description="Ensures a null resource name is not considered an embedded page.")]
		public void IsEmbeddedPageResource_Null()
		{
			bool actual = this.IsEmbeddedPageResource(null);
			Assert.IsFalse(actual, "Null resource name should not be a page.");
		}

		[Test(Description="Ensures a valid resource name that is all lower case is considered an embedded page.")]
		public void IsEmbeddedPageResource_Lowercase()
		{
			bool actual = this.IsEmbeddedPageResource("foo.aspx");
			Assert.IsTrue(actual, "Lower case extension should be accepted as a page.");
		}

		[Test(Description="Ensures a valid resource name that is mixed case is considered an embedded page.")]
		public void IsEmbeddedPageResource_Mixedcase()
		{
			bool actual = this.IsEmbeddedPageResource("foo.AsPx");
			Assert.IsTrue(actual, "Mixed case extension should be accepted as a page.");
		}

		[Test(Description="Ensures a valid resource name that is all upper case is considered an embedded page.")]
		public void IsEmbeddedPageResource_Uppercase()
		{
			bool actual = this.IsEmbeddedPageResource("foo.ASPX");
			Assert.IsTrue(actual, "Upper case extension should be accepted as a page.");
		}

		#endregion

		#region MapResourceToFileSystem

		[Test(Description="Maps a valid resource path into the filesystem.")]
		public void MapResourceToFileSystem_Valid()
		{
			string baseFolder = @"C:\temp";
			string resourcePath = "MyNamespace.MySubnamespace.MyFolder1.MyFolder2.MyFile.txt";
			string baseNamespace = "MyNamespace.MySubnamespace";
			string expected = @"C:\temp\MyFolder1\MyFolder2\MyFile.txt";
			string actual = this.MapResourceToFileSystem(baseNamespace, resourcePath, baseFolder);
			Assert.AreEqual(expected, actual, "Resource was not correctly mapped into the filesystem.");
		}

		[Test(Description="Attempts to map a resource path using a null base namespace.")]
		[ExpectedException(typeof(System.ArgumentNullException))]
		public void MapResourceToFileSystem_Arg_BaseNamespace1()
		{
			string actual = this.MapResourceToFileSystem(null, "MyNamespace.MySubnamespace.MyFolder1.MyFolder2.MyFile.txt", @"C:\temp");
		}

		[Test(Description="Attempts to map a resource path using an empty base namespace.")]
		[ExpectedException(typeof(System.ArgumentOutOfRangeException))]
		public void MapResourceToFileSystem_Arg_BaseNamespace2()
		{
			string actual = this.MapResourceToFileSystem("", "MyNamespace.MySubnamespace.MyFolder1.MyFolder2.MyFile.txt", @"C:\temp");
		}

		[Test(Description="Attempts to map a resource path using a base namespace that does not appear at all in the resource path.")]
		[ExpectedException(typeof(System.InvalidOperationException))]
		public void MapResourceToFileSystem_Arg_BaseNamespace3()
		{
			string baseFolder = @"C:\temp";
			string resourcePath = "MyNamespace.MySubnamespace.MyFolder1.MyFolder2.MyFile.txt";
			string baseNamespace = "SomeOtherNamespace";
			string actual = this.MapResourceToFileSystem(baseNamespace, resourcePath, baseFolder);
		}

		[Test(Description="Attempts to map a resource path using a base namespace that appears in the resource path, but not at the start.")]
		[ExpectedException(typeof(System.InvalidOperationException))]
		public void MapResourceToFileSystem_Arg_BaseNamespace4()
		{
			string baseFolder = @"C:\temp";
			string resourcePath = "MyNamespace.MySubnamespace.MyFolder1.MyFolder2.MyFile.txt";
			string baseNamespace = "MySubnamespace";
			string actual = this.MapResourceToFileSystem(baseNamespace, resourcePath, baseFolder);
		}

		[Test(Description="Attempts to map a resource path using a base namespace that has two periods together.")]
		[ExpectedException(typeof(System.ArgumentOutOfRangeException))]
		public void MapResourceToFileSystem_Arg_BaseNamespace5()
		{
			string baseFolder = @"C:\temp";
			string resourcePath = "MyNamespace.MySubnamespace.MyFolder1.MyFolder2.MyFile.txt";
			string baseNamespace = "MyNamespace..MySubnamespace";
			string actual = this.MapResourceToFileSystem(baseNamespace, resourcePath, baseFolder);
		}

		[Test(Description="Attempts to map a resource path using a base namespace that has starts with a period.")]
		[ExpectedException(typeof(System.ArgumentOutOfRangeException))]
		public void MapResourceToFileSystem_Arg_BaseNamespace6()
		{
			string baseFolder = @"C:\temp";
			string resourcePath = "MyNamespace.MySubnamespace.MyFolder1.MyFolder2.MyFile.txt";
			string baseNamespace = ".MyNamespace.MySubnamespace";
			string actual = this.MapResourceToFileSystem(baseNamespace, resourcePath, baseFolder);
		}

		[Test(Description="Attempts to map a resource path using a base namespace that has ends with a period.")]
		[ExpectedException(typeof(System.ArgumentOutOfRangeException))]
		public void MapResourceToFileSystem_Arg_BaseNamespace7()
		{
			string baseFolder = @"C:\temp";
			string resourcePath = "MyNamespace.MySubnamespace.MyFolder1.MyFolder2.MyFile.txt";
			string baseNamespace = "MyNamespace.MySubnamespace.";
			string actual = this.MapResourceToFileSystem(baseNamespace, resourcePath, baseFolder);
		}

		[Test(Description="Attempts to map a resource path using a null resource path.")]
		[ExpectedException(typeof(System.ArgumentNullException))]
		public void MapResourceToFileSystem_Arg_ResourcePath1()
		{
			string actual = this.MapResourceToFileSystem("MyNamespace.MySubnamespace", null, @"C:\temp");
		}

		[Test(Description="Attempts to map a resource path using an empty resource path.")]
		[ExpectedException(typeof(System.ArgumentOutOfRangeException))]
		public void MapResourceToFileSystem_Arg_ResourcePath2()
		{
			string actual = this.MapResourceToFileSystem("MyNamespace.MySubnamespace", "", @"C:\temp");
		}

		[Test(Description="Attempts to map a resource path using a resource path that has two periods together.")]
		[ExpectedException(typeof(System.ArgumentOutOfRangeException))]
		public void MapResourceToFileSystem_Arg_ResourcePath3()
		{
			string baseFolder = @"C:\temp";
			string resourcePath = "MyNamespace.MySubnamespace..MyFolder1.MyFolder2.MyFile.txt";
			string baseNamespace = "MyNamespace.MySubnamespace";
			string actual = this.MapResourceToFileSystem(baseNamespace, resourcePath, baseFolder);
		}

		[Test(Description="Attempts to map a resource path using a resource path that starts with a period.")]
		[ExpectedException(typeof(System.ArgumentOutOfRangeException))]
		public void MapResourceToFileSystem_Arg_ResourcePath4()
		{
			string baseFolder = @"C:\temp";
			string resourcePath = ".MyNamespace.MySubnamespace.MyFolder1.MyFolder2.MyFile.txt";
			string baseNamespace = "MyNamespace.MySubnamespace";
			string actual = this.MapResourceToFileSystem(baseNamespace, resourcePath, baseFolder);
		}

		[Test(Description="Attempts to map a resource path using a resource path that ends with a period.")]
		[ExpectedException(typeof(System.ArgumentOutOfRangeException))]
		public void MapResourceToFileSystem_Arg_ResourcePath5()
		{
			string baseFolder = @"C:\temp";
			string resourcePath = "MyNamespace.MySubnamespace.MyFolder1.MyFolder2.MyFile.txt.";
			string baseNamespace = "MyNamespace.MySubnamespace";
			string actual = this.MapResourceToFileSystem(baseNamespace, resourcePath, baseFolder);
		}

		[Test(Description="Maps a valid resource path into the filesystem using a null base folder.")]
		public void MapResourceToFileSystem_Arg_BaseFolder1()
		{
			string resourcePath = "MyNamespace.MySubnamespace.MyFolder1.MyFolder2.MyFile.txt";
			string baseNamespace = "MyNamespace.MySubnamespace";
			string expected = Path.Combine(Environment.CurrentDirectory, @"MyFolder1\MyFolder2\MyFile.txt");
			string actual = this.MapResourceToFileSystem(baseNamespace, resourcePath, null);
			Assert.AreEqual(expected, actual, "Resource was not correctly mapped into the filesystem with null base folder.");
		}

		[Test(Description="Maps a valid resource path into the filesystem using an empty base folder.")]
		public void MapResourceToFileSystem_Arg_BaseFolder2()
		{
			string resourcePath = "MyNamespace.MySubnamespace.MyFolder1.MyFolder2.MyFile.txt";
			string baseNamespace = "MyNamespace.MySubnamespace";
			string expected = Path.Combine(Environment.CurrentDirectory, @"MyFolder1\MyFolder2\MyFile.txt");
			string actual = this.MapResourceToFileSystem(baseNamespace, resourcePath, "");
			Assert.AreEqual(expected, actual, "Resource was not correctly mapped into the filesystem with empty base folder.");
		}

		#endregion

		#region ReleaseHandler

		[Test(Description="Ensures no exceptions are thrown when releasing a null handler.")]
		public void ReleaseHandler_Null()
		{
			this._factory.ReleaseHandler(null);
		}

		#endregion

		#endregion


		#region Helpers

		/// <summary>
		/// Calls the ApplicationDisposed method on the factory test instance.
		/// </summary>
		public void ApplicationDisposed()
		{
			this.InvokeNonPublicMember("ApplicationDisposed", new object[]{null, null});
		}

		/// <summary>
		/// Asserts that a specific number of files and folders exist within a path.
		/// </summary>
		/// <param name="path">The path to check for files/folders.</param>
		/// <param name="numFiles">Number of files expected in the path (including all nested children).</param>
		/// <param name="numFolders">Number of folders expected in the path (including all nested children).</param>
		public void AssertPathContents(string path, int numFiles, int numFolders)
		{
			Assert.IsTrue(Directory.Exists(path), String.Format(CultureInfo.InvariantCulture, "The path to check ({0}) does not exist.", path));

			Stack pathsToCheck = new Stack();
			int actualFolders = 0;
			int actualFiles = 0;

			// Push the root onto the stack
			pathsToCheck.Push(path);

			// Count files and folders
			while(pathsToCheck.Count > 0)
			{
				string folder = (string)pathsToCheck.Pop();
				actualFolders++;
				string[] subFolders = Directory.GetDirectories(folder);
				foreach(string subFolder in subFolders)
				{
					pathsToCheck.Push(subFolder);
				}
				actualFiles += Directory.GetFiles(folder).Length;
			}

			// Subtract one for the root
			actualFolders--;

			// Make assertions
			Assert.AreEqual(numFiles, actualFiles, "An incorrect number of files was found.");
			Assert.AreEqual(numFolders, actualFolders, "An incorrect number of folders was found.");
		}

		/// <summary>
		/// Calls the ConvertServerPathToTempPath method on the factory test instance.
		/// </summary>
		/// <param name="path">The mapped path on the server.</param>
		/// <returns>
		/// A <see cref="System.String"/> with the path to the corresponding file
		/// in the temporary filesystem.
		/// </returns>
		public string ConvertServerPathToTempPath(string path)
		{
			return (string)this.InvokeNonPublicMember("ConvertServerPathToTempPath", new object[]{path});
		}

		/// <summary>
		/// Calls the ExtractResourceToFile method on the factory test instance.
		/// </summary>
		/// <param name="assembly">The <see cref="System.Reflection.Assembly"/> containing the resource.</param>
		/// <param name="resourcePath">The path to the embedded resource in the <paramref name="assembly" />.</param>
		/// <param name="destinationPath">The absolute path of the destination in the filesystem where the resource should be extracted.</param>
		public void ExtractResourceToFile(Assembly assembly, string resourcePath, string destinationPath)
		{
			this.InvokeNonPublicMember("ExtractResourceToFile", new object[]{assembly, resourcePath, destinationPath});
		}

		/// <summary>
		/// Calls the GetConfiguredAssemblySection method on the factory test instance.
		/// </summary>
		/// <returns>
		/// An <see cref="System.Collections.IDictionary"/> where each key indicates
		/// the name of an assembly with embedded pages and the corresponding value
		/// is the root namespace of the assembly.
		/// </returns>
		public IDictionary GetConfiguredAssemblySection()
		{
			return this.InvokeNonPublicMember("GetConfiguredAssemblySection", new object[]{}) as IDictionary;
		}

		/// <summary>
		/// Calls the GetEmbeddedPageResourceNames method on the factory test instance.
		/// </summary>
		/// <param name="assembly">The assembly to enumerate the embedded pages in.</param>
		/// <returns>
		/// An array of resource names indicating the embedded web forms in the
		/// provided assembly.
		/// </returns>
		public string[] GetEmbeddedPageResourceNames(Assembly assembly)
		{
			return (string[])this.InvokeNonPublicMember("GetEmbeddedPageResourceNames", new object[]{assembly});
		}

		/// <summary>
		/// Retrieves the value of a non-public <see langword="static" /> field.
		/// </summary>
		/// <param name="fieldName">The name of the field to return.</param>
		/// <returns>
		/// The value of the non-public <see langword="static" /> field.
		/// </returns>
		public object GetNonPublicStaticFieldValue(string fieldName)
		{
			return typeof(EPHF).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
		}

		/// <summary>
		/// Invokes a non-public member on the factory test instance.
		/// </summary>
		/// <param name="memberName">The name of the member to invoke.</param>
		/// <param name="args">The arguments to pass to the method.</param>
		/// <returns>
		/// The output of invoking the member.
		/// </returns>
		public object InvokeNonPublicMember(string memberName, object[] args)
		{
			try
			{
				return typeof(EPHF).InvokeMember(memberName, BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.NonPublic, null, this._factory, args);
			}
			catch(TargetInvocationException err)
			{
				if(err.InnerException != null)
				{
					throw err.InnerException;
				}
				else
				{
					throw;
				}
			}
		}

		/// <summary>
		/// Invokes a non-public <see langword="static" /> member on the factory test instance.
		/// </summary>
		/// <param name="memberName">The name of the member to invoke.</param>
		/// <param name="args">The arguments to pass to the method.</param>
		/// <returns>
		/// The output of invoking the member.
		/// </returns>
		public object InvokeNonPublicStaticMember(string memberName, object[] args)
		{
			try
			{
				return typeof(EPHF).InvokeMember(memberName, BindingFlags.Static | BindingFlags.InvokeMethod | BindingFlags.NonPublic, null, null, args);
			}
			catch(TargetInvocationException err)
			{
				if(err.InnerException != null)
				{
					throw err.InnerException;
				}
				else
				{
					throw;
				}
			}
		}

		/// <summary>
		/// Calls the IsEmbeddedPageResource method on the factory test instance.
		/// </summary>
		/// <param name="resourceName">The name of the embedded resource to check.</param>
		/// <returns>The mapped path of the resource into the target folder.</returns>
		public bool IsEmbeddedPageResource(string resourceName)
		{
			return (bool)this.InvokeNonPublicMember("IsEmbeddedPageResource", new object[]{resourceName});
		}

		/// <summary>
		/// Calls the MapResourceToFileSystem method on the factory test instance.
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
		public string MapResourceToFileSystem(string baseNamespace, string resourcePath, string baseFolder)
		{
			return (string)this.InvokeNonPublicMember("MapResourceToFileSystem", new object[]{baseNamespace, resourcePath, baseFolder});
		}

		/// <summary>
		/// Gets or sets the private static ModuleInitialized member.
		/// </summary>
		public bool ModuleInitialized
		{
			get 
			{
				return (bool)this.GetNonPublicStaticFieldValue("ModuleInitialized"); 
			}
			set
			{
				this.SetNonPublicStaticFieldValue("ModuleInitialized", value);
			}
		}

		/// <summary>
		/// Sets the value of a non-public <see langword="static" /> field.
		/// </summary>
		/// <param name="fieldName">The name of the field to set.</param>
		/// <param name="val">The value to set the field to.</param>
		public void SetNonPublicStaticFieldValue(string fieldName, object val)
		{
			typeof(EPHF).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Static).SetValue(null, val);
		}

		/// <summary>
		/// Gets the private static TempFileBasePath member.
		/// </summary>
		public string TempFileBasePath
		{
			get 
			{
				return (string)this.GetNonPublicStaticFieldValue("TempFileBasePath"); 
			}
		}

		#endregion
	
	}
}
