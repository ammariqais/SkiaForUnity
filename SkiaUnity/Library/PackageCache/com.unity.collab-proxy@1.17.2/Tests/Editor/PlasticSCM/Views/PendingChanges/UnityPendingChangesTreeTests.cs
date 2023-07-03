using System.Collections.Generic;

using NUnit.Framework;

using Codice.Client.BaseCommands;
using Codice.Client.Commands;
using Codice.CM.Common;
using PlasticGui.WorkspaceWindow.PendingChanges;
using Unity.PlasticSCM.Editor.Views.PendingChanges;

namespace Unity.PlasticSCM.Tests.Editor.Views.PendingChanges
{
    [TestFixture]
    class UnityPendingChangesTreeTests
    {
        [Test]
        public void TestAddedNoMeta()
        {
            UnityPendingChangesTree tree = new UnityPendingChangesTree();

            WorkspaceInfo wkInfo = new WorkspaceInfo("foo", "/foo");

            ChangeInfo added = new ChangeInfo()
            {
                Path = "/foo/foo.c"
            };

            List<ChangeInfo> changes = new List<ChangeInfo>();
            changes.Add(added);

            tree.BuildChangeCategories(
                wkInfo,
                changes,
                new CheckedStateManager());

            Assert.IsNull(
                tree.GetMetaChange(added),
                "Meta change should be null");
        }

        [Test]
        public void TestAddedWithMeta()
        {
            UnityPendingChangesTree tree = new UnityPendingChangesTree();

            WorkspaceInfo wkInfo = new WorkspaceInfo("foo", "/foo");

            ChangeInfo added = new ChangeInfo()
            {
                Path = "/foo/foo.c"
            };

            ChangeInfo addedMeta = new ChangeInfo()
            {
                Path = "/foo/foo.c.meta"
            };

            List<ChangeInfo> changes = new List<ChangeInfo>();
            changes.Add(added);
            changes.Add(addedMeta);

            tree.BuildChangeCategories(
                wkInfo,
                changes,
                new CheckedStateManager());

            Assert.IsTrue(
                changes.Contains(added),
                "Pending changes should contain the change");

            Assert.IsFalse(
                changes.Contains(addedMeta),
                "Pending changes should not contain the meta");

            Assert.AreEqual(addedMeta, tree.GetMetaChange(added));
        }

        [Test]
        public void TestFileAddedMetaPrivate()
        {
            UnityPendingChangesTree tree = new UnityPendingChangesTree();

            WorkspaceInfo wkInfo = new WorkspaceInfo("foo", "/foo");

            ChangeInfo added = new ChangeInfo()
            {
                Path = "/foo/foo.c",
                ChangeTypes = ChangeTypes.Added,
            };

            ChangeInfo privateMeta = new ChangeInfo()
            {
                Path = "/foo/foo.c.meta",
                ChangeTypes = ChangeTypes.Private,
            };

            List<ChangeInfo> changes = new List<ChangeInfo>();
            changes.Add(added);
            changes.Add(privateMeta);

            tree.BuildChangeCategories(
                wkInfo,
                changes,
                new CheckedStateManager());

            Assert.IsTrue(
                changes.Contains(added),
                "Pending changes should contain the change");

            Assert.IsTrue(
                changes.Contains(privateMeta),
                "Pending changes should contain the meta");
        }

        [Test]
        public void TestDeletedNoMeta()
        {
            UnityPendingChangesTree tree = new UnityPendingChangesTree();

            WorkspaceInfo wkInfo = new WorkspaceInfo("foo", "/foo");

            ChangeInfo deleted = new ChangeInfo()
            {
                Path = "/foo/foo.c"
            };

            List<ChangeInfo> changes = new List<ChangeInfo>();
            changes.Add(deleted);

            tree.BuildChangeCategories(
                wkInfo,
                changes,
                new CheckedStateManager());

            Assert.IsNull(
                tree.GetMetaChange(deleted),
                "Meta change should be null");
        }

        [Test]
        public void TestDeletedWithMeta()
        {
            UnityPendingChangesTree tree = new UnityPendingChangesTree();

            WorkspaceInfo wkInfo = new WorkspaceInfo("foo", "/foo");

            ChangeInfo deleted = new ChangeInfo()
            {
                Path = "/foo/foo.c"
            };

            ChangeInfo deletedMeta = new ChangeInfo()
            {
                Path = "/foo/foo.c.meta"
            };

            List<ChangeInfo> changes = new List<ChangeInfo>();
            changes.Add(deleted);
            changes.Add(deletedMeta);

            tree.BuildChangeCategories(
                wkInfo,
                changes,
                new CheckedStateManager());

            Assert.IsTrue(
                changes.Contains(deleted),
                "Pending changes should contain the change");

            Assert.IsFalse(
                changes.Contains(deletedMeta),
                "Pending changes should not contain the meta");

            Assert.AreEqual(deletedMeta, tree.GetMetaChange(deleted));
        }

        [Test]
        public void TestChangedNoMeta()
        {
            UnityPendingChangesTree tree = new UnityPendingChangesTree();

            WorkspaceInfo wkInfo = new WorkspaceInfo("foo", "/foo");

            ChangeInfo changed = new ChangeInfo()
            {
                Path = "/foo/foo.c"
            };

            List<ChangeInfo> changes = new List<ChangeInfo>();
            changes.Add(changed);

            tree.BuildChangeCategories(
                wkInfo,
                changes,
                new CheckedStateManager());

            Assert.IsNull(
                tree.GetMetaChange(changed),
                "Meta change should be null");
        }

        [Test]
        public void TestChangedWithMeta()
        {
            UnityPendingChangesTree tree = new UnityPendingChangesTree();

            WorkspaceInfo wkInfo = new WorkspaceInfo("foo", "/foo");

            ChangeInfo changed = new ChangeInfo()
            {
                Path = "/foo/foo.c"
            };

            ChangeInfo changedMeta = new ChangeInfo()
            {
                Path = "/foo/foo.c.meta"
            };

            List<ChangeInfo> changes = new List<ChangeInfo>();
            changes.Add(changed);
            changes.Add(changedMeta);

            tree.BuildChangeCategories(
                wkInfo,
                changes,
                new CheckedStateManager());

            Assert.IsTrue(
                changes.Contains(changed),
                "Pending changes should contain the change");

            Assert.IsFalse(
                changes.Contains(changedMeta),
                "Pending changes should not contain the meta");

            Assert.AreEqual(changedMeta, tree.GetMetaChange(changed));
        }

        [Test]
        public void TestMovedNoMeta()
        {
            UnityPendingChangesTree tree = new UnityPendingChangesTree();

            WorkspaceInfo wkInfo = new WorkspaceInfo("foo", "/foo");

            ChangeInfo moved = new ChangeInfo()
            {
                OldPath = "/foo/foo.c",
                Path = "/foo/bar/newfoo.c",
            };

            List<ChangeInfo> changes = new List<ChangeInfo>();
            changes.Add(moved);

            tree.BuildChangeCategories(
                wkInfo,
                changes,
                new CheckedStateManager());

            Assert.IsNull(
                tree.GetMetaChange(moved),
                "Meta change should be null");
        }

        [Test]
        public void TestMovedWithMeta()
        {
            UnityPendingChangesTree tree = new UnityPendingChangesTree();

            WorkspaceInfo wkInfo = new WorkspaceInfo("foo", "/foo");

            ChangeInfo moved = new ChangeInfo()
            {
                OldPath = "/foo/foo.c",
                Path = "/foo/bar/newfoo.c",
            };

            ChangeInfo movedMeta = new ChangeInfo()
            {
                OldPath = "/foo/foo.c.meta",
                Path = "/foo/bar/newfoo.c.meta",
            };

            List<ChangeInfo> changes = new List<ChangeInfo>();
            changes.Add(moved);
            changes.Add(movedMeta);

            tree.BuildChangeCategories(
                wkInfo,
                changes,
                new CheckedStateManager());

            Assert.IsTrue(
                changes.Contains(moved),
                "Pending changes should contain the change");

            Assert.IsFalse(
                changes.Contains(movedMeta),
                "Pending changes should not contain the meta");

            Assert.AreEqual(movedMeta, tree.GetMetaChange(moved));
        }

        [Test]
        public void TestOnlyMeta()
        {
            UnityPendingChangesTree tree = new UnityPendingChangesTree();

            WorkspaceInfo wkInfo = new WorkspaceInfo("foo", "/foo");

            ChangeInfo addedMeta = new ChangeInfo()
            {
                Path = "/foo/foo.c.meta",
            };

            List<ChangeInfo> changes = new List<ChangeInfo>();
            changes.Add(addedMeta);

            tree.BuildChangeCategories(
                wkInfo,
                changes,
                new CheckedStateManager());

            Assert.IsNull(
                tree.GetMetaChange(addedMeta),
                "Meta change should be null");
        }

        [Test]
        public void TestCheckedChangesWithMeta()
        {
            UnityPendingChangesTree tree = new UnityPendingChangesTree();

            WorkspaceInfo wkInfo = new WorkspaceInfo("foo", "/foo");

            ChangeInfo added = new ChangeInfo()
            {
                Path = "/foo/foo.c",
                ChangeTypes = ChangeTypes.Added
            };

            ChangeInfo addedMeta = new ChangeInfo()
            {
                Path = "/foo/foo.c.meta",
                ChangeTypes = ChangeTypes.Added
            };

            List<ChangeInfo> changes = new List<ChangeInfo>();
            changes.Add(added);
            changes.Add(addedMeta);

            CheckedStateManager checkedStateManager =
                new CheckedStateManager();

            tree.BuildChangeCategories(
                wkInfo,
                changes,
                checkedStateManager);

            checkedStateManager.Update(added, true);

            List<ChangeInfo> dependenciesCandidates;

            tree.GetCheckedChanges(
                null,
                false,
                out changes,
                out dependenciesCandidates);

            Assert.IsTrue(changes.Contains(added),
                "Changes should contains the change");

            Assert.IsTrue(changes.Contains(addedMeta),
                "Changes should contains the meta change");
        }

        [Test]
        public void TestCheckedChangesWithDependenciesCandidates()
        {
            UnityPendingChangesTree tree = new UnityPendingChangesTree();

            WorkspaceInfo wkInfo = new WorkspaceInfo("foo", "/foo");

            ChangeInfo dir = new ChangeInfo()
            {
                Path = "/foo/bar",
                ChangeTypes = ChangeTypes.Changed
            };

            ChangeInfo dirMeta = new ChangeInfo()
            {
                Path = "/foo/bar.meta",
                ChangeTypes = ChangeTypes.Changed
            };


            ChangeInfo added = new ChangeInfo()
            {
                Path = "/foo/bar/foo.c",
                ChangeTypes = ChangeTypes.Added
            };

            ChangeInfo addedMeta = new ChangeInfo()
            {
                Path = "/foo/bar/foo.c.meta",
                ChangeTypes = ChangeTypes.Added
            };

            List<ChangeInfo> changes = new List<ChangeInfo>();
            changes.Add(dir);
            changes.Add(dirMeta);
            changes.Add(added);
            changes.Add(addedMeta);

            CheckedStateManager checkedStateManager = new CheckedStateManager();

            tree.BuildChangeCategories(
                wkInfo,
                changes,
                checkedStateManager);

            checkedStateManager.Update(added, true);
            checkedStateManager.Update(dir, false);

            List<ChangeInfo> dependenciesCandidates;

            tree.GetCheckedChanges(
                null,
                false,
                out changes,
                out dependenciesCandidates);

            Assert.IsTrue(changes.Contains(added),
                "Changes should contains the change");

            Assert.IsTrue(changes.Contains(addedMeta),
                "Changes should contains the meta change");

            Assert.IsTrue(dependenciesCandidates.Contains(dir),
                "Dependencies candidates should contains the dir");

            Assert.IsTrue(dependenciesCandidates.Contains(dirMeta),
                "Dependencies candidates should contains the meta dir");
        }
    }
}
