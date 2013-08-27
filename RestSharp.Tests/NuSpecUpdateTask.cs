﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using System.Reflection;
using System.IO;
using System.Xml.Linq;

namespace RestSharp.Tests
{
    public class NuSpecUpdateTask
    {
        public abstract class BaseNuSpecUpdateTest
        {
            protected string FileName { get; set; }

            protected string ComputedFileName
            {
                get { return this.FileName.Replace(".nuspec", "-computed.nuspec"); }
            }

            protected BaseNuSpecUpdateTest()
            {
                this.FileName = Path.Combine("SampleData", "restsharp.nuspec");
                this.Setup();
            }

            protected abstract void Setup();
        }

        public class Execute
        {
            public class WhenSpecFileNotSpecified
            {
                [Fact]
                public void ReturnsFalse()
                {
                    var task = new Automation.NuSpecUpdateTask();
                    Assert.False(task.Execute());
                }
            }

            public class WhenInformationalVersionIsNotDefined : BaseNuSpecUpdateTest
            {
                protected override void Setup() { }

                [Fact]
                public void PullsVersionAttributeInstead()
                {
                    var task = new Automation.NuSpecUpdateTask(this.GetType().Assembly);
                    task.SpecFile = this.FileName;
                    task.Execute();

                    Assert.Equal("1.0.0.0", task.Version);
                }                
            }

            public class WhenSpecFileIsValid : BaseNuSpecUpdateTest
            {
                private Automation.NuSpecUpdateTask _subject = new Automation.NuSpecUpdateTask();
                private bool _result;

                private string _expectedId = "RestSharp";
                private string _expectedDescription = "Simple REST and HTTP API Client";
                private string _expectedAuthors = "John Sheehan, RestSharp Community";
                private string _expectedOwners = "John Sheehan, RestSharp Community";
                private string _expectedVersion = "104.2.0";

                protected override void Setup()
                {
                    this._subject.SpecFile = this.FileName;
                    this._result = this._subject.Execute();
                }

                [Fact]
                public void ReturnsTrue()
                {
                    Assert.True(this._result);
                }

                [Fact]
                public void PullsIdFromAssembly()
                {
                    Assert.Equal(this._expectedId, this._subject.Id);
                }

                [Fact]
                public void PullsDescriptionFromAssembly()
                {
                    Assert.Equal(this._expectedDescription, this._subject.Description);
                }

                [Fact]
                public void PullsVersionFromAssemblyInfo()
                {
                    Assert.Equal(this._expectedVersion, this._subject.Version);
                }

                [Fact]
                public void PullsAuthorsFromAssemblyInfo()
                {
                    Assert.Equal(this._expectedAuthors, this._subject.Authors);
                }

                [Fact]
                public void UpdatesSpecFile()
                {
                    var doc = XDocument.Load(this.ComputedFileName);
                    Assert.Equal(this._expectedId, doc.Descendants("id").First().Value);
                    Assert.Equal(this._expectedDescription, doc.Descendants("description").First().Value);
                    Assert.Equal(this._expectedAuthors, doc.Descendants("authors").First().Value);
                    Assert.Equal(this._expectedOwners, doc.Descendants("owners").First().Value);
                    Assert.Equal(this._expectedVersion, doc.Descendants("version").First().Value);
                }
            }
        }
    }
}
