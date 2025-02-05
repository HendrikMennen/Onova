﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Onova.Services;
using Onova.Tests.Internal;

namespace Onova.Tests
{
    [TestFixture]
    public class LocalPackageResolverTests
    {
        private static string TempDirPath => Path.Combine(TestContext.CurrentContext.TestDirectory, "Temp");

        private static LocalPackageResolver CreateLocalPackageResolver(IReadOnlyDictionary<Version, byte[]> packages)
        {
            foreach (var package in packages)
            {
                var packageFilePath = Path.Combine(TempDirPath, $"{package.Key}.onv");
                File.WriteAllBytes(packageFilePath, package.Value);
            }

            return new LocalPackageResolver(TempDirPath, "*.onv");
        }

        private static LocalPackageResolver CreateLocalPackageResolver(IReadOnlyList<Version> versions) =>
            CreateLocalPackageResolver(versions.ToDictionary(v => v, _ => new byte[] {1, 2, 3, 4, 5}));

        [SetUp]
        public void Setup()
        {
            // Ensure temp directory exists and is empty
            DirectoryEx.Reset(TempDirPath);
        }

        [TearDown]
        public void Cleanup()
        {
            // Delete temp directory
            if (Directory.Exists(TempDirPath))
                Directory.Delete(TempDirPath, true);
        }

        [Test]
        public async Task GetPackageVersionsAsync_Test()
        {
            // Arrange
            var availableVersions = new[]
            {
                Version.Parse("1.0"),
                Version.Parse("2.0"),
                Version.Parse("3.0")
            };

            var resolver = CreateLocalPackageResolver(availableVersions);

            // Act
            var versions = await resolver.GetPackageVersionsAsync();

            // Assert
            Assert.That(versions, Is.EquivalentTo(availableVersions));
        }

        [Test]
        public async Task DownloadPackageAsync_Test()
        {
            // Arrange
            var availablePackages = new Dictionary<Version, byte[]>
            {
                {Version.Parse("1.0"), new byte[] {1, 2, 3}},
                {Version.Parse("2.0"), new byte[] {4, 5, 6}},
                {Version.Parse("3.0"), new byte[] {7, 8, 9}}
            };

            var selectedPackage = availablePackages.Last();
            var destFilePath = Path.Combine(TempDirPath, "Output.onv");

            var resolver = CreateLocalPackageResolver(availablePackages);

            // Act
            await resolver.DownloadPackageAsync(selectedPackage.Key, destFilePath);

            // Assert
            Assert.That(File.Exists(destFilePath), "File exists");
            Assert.That(File.ReadAllBytes(destFilePath), Is.EqualTo(selectedPackage.Value), "File content");
        }
    }
}