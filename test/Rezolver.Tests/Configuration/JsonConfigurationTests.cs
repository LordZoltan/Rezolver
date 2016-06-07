﻿using Rezolver.Configuration;
using Rezolver.Configuration.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests.Configuration
{
	public class JsonConffigurationTests : JsonConfigurationTestsBase
	{
		[Fact]
		public void ShouldCreateJsonConfigurationFromJsonString()
		{
			string json = @"{
	""assemblies"": [
		""Rezolver.Tests""
	],
	""rezolve"": [
		{ ""System.Int32"": 10 },
		{ ""Rezolver.Tests.TestTypes.RequiresInt"": { ""$construct"": ""$auto"" } },
		{ ""Rezolver.Tests.TestTypes.IRequiresInt"": { ""$construct"": ""Rezolver.Tests.TestTypes.RequiresInt"" } },
		{
			""type"": { ""name"": ""System.Collections.Generic.IEnumerable"", ""args"": [ ""System.Int32"" ] },
			""value"": [ 1, 2, 3 ]
		},
		{
			""types"": [ ""System.Object"", ""System.String"" ],
			""value"": ""Hello world""
		}
	]
}";

			var parser = new JsonConfigurationParser();
			IConfiguration configuration = parser.Parse(json);
			Assert.IsType<JsonConfiguration>(configuration);
		}


		[Fact]
		public void AdapterShouldBuildRezolverBuilder()
		{
			string json = @"{
	""assemblies"": [
		""Rezolver.Tests""
	],
	""rezolve"": [
		{ ""System.Int32"": 10 },
		{ ""Rezolver.Tests.TestTypes.RequiresInt"": { ""$construct"": ""$auto"" } },
		{ ""Rezolver.Tests.TestTypes.IRequiresInt"": { ""$construct"": ""Rezolver.Tests.TestTypes.RequiresInt"" } },
		{
			""type"": { ""name"": ""System.Collections.Generic.IEnumerable"", ""args"": [ ""System.Int32"" ] },
			""value"": [ 1, 2, 3 ]
		},
		{
			""types"": [ ""System.Object"", ""System.String"" ],
			""value"": ""Hello world""
		}
	]
}";
			var parser = new JsonConfigurationParser();

			IConfiguration configuration = parser.Parse(json);
			//use the defaul adapter
			IConfigurationAdapter adapter = new ConfigurationAdapter();
			var builder = adapter.CreateBuilder(configuration);

			Assert.IsType<Builder>(builder);

			var rezolver = new Container(builder, new TargetDelegateCompiler());
			var str = rezolver.Resolve<string>();
			Assert.Equal("Hello world", str);
			var en = rezolver.Resolve<IEnumerable<int>>();
			Assert.NotNull(en);
			Assert.True(en.SequenceEqual(new[] { 1, 2, 3 }));
		}
	}
}
