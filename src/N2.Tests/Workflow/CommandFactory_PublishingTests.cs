﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using N2.Workflow;
using N2.Security;
using N2.Edit;
using Rhino.Mocks;
using N2.Workflow.Commands;
using N2.Tests.Workflow.Items;

namespace N2.Tests.Workflow
{
    [TestFixture]
    public class CommandFactory_PublishingTests : CommandFactoryTestsBase
    {

        protected override CommandBase<CommandContext> CreateCommand(CommandContext context)
        {
            return commands.GetPublishCommand(context);
        }

        [Test]
        public void IsNotValidated_WhenInterface_IsViewing()
        {
            item = MakeVersion(item);

			var validator = MockRepository.GenerateStub<IValidator<CommandContext>>();
            mocks.ReplayAll();

            var context = new CommandContext(item, Interfaces.Viewing, CreatePrincipal("admin"), nullBinder, validator);

            var command = CreateCommand(context);
            dispatcher.Execute(command, context);

            validator.AssertWasNotCalled(b => b.Validate(context));
        }

        [Test]
        public void MakesVersion_OfCurrent()
        {
            var context = new CommandContext(item, Interfaces.Editing, CreatePrincipal("admin"), nullBinder, nullValidator);

            var command = CreateCommand(context);
            dispatcher.Execute(command, context);

            Assert.That(repository.database.Values.Count(v => v.VersionOf == item), Is.EqualTo(1));
        }

        [Test]
        public void Version_MakesVersion_OfCurrentMaster()
        {
            var version = MakeVersion(item);
            var context = new CommandContext(version, Interfaces.Editing, CreatePrincipal("admin"), nullBinder, nullValidator);

            var command = CreateCommand(context);
            dispatcher.Execute(command, context);

            Assert.That(repository.database.Values.Count(v => v.VersionOf == item), Is.EqualTo(1));
            Assert.That(item.Title, Is.EqualTo("version"));
        }

        [Test]
        public void Version_Replaces_MasterVersion()
        {
            var version = MakeVersion(item);
            version.Name = "tha masta";
            var context = new CommandContext(version, Interfaces.Editing, CreatePrincipal("admin"), nullBinder, nullValidator);

            var command = CreateCommand(context);
            dispatcher.Execute(command, context);

            Assert.That(context.Content.Name, Is.EqualTo("tha masta"));
            Assert.That(context.Content.VersionOf, Is.Null);
        }

        [TestCase(Interfaces.Viewing, true)]
        [TestCase(Interfaces.Editing, false)]
        [TestCase(Interfaces.Editing, true)]
        public void RedirectsTo_PreviewUrl(string userInterface, bool useVersion)
        {
            var version = useVersion ? MakeVersion(item) : item;
            var context = new CommandContext(version, userInterface, CreatePrincipal("admin"), nullBinder, nullValidator);

            var command = CreateCommand(context);
            dispatcher.Execute(command, context);

            Assert.That(context.RedirectTo, Is.EqualTo(((INode)item).PreviewUrl));
        }

        [Test]
        public void PreviouslyPublishedVersion_CausesNewVersion_FromView()
        {
            var version = MakeVersion(item);
            version.State = ContentState.Unpublished;

            var context = new CommandContext(version, Interfaces.Viewing, CreatePrincipal("admin"), nullBinder, nullValidator);

            var command = CreateCommand(context);
            dispatcher.Execute(command, context);

            Assert.That(versions.GetVersionsOf(item).Count, Is.EqualTo(3));
        }

        [Test]
        public void RedirectsTo_OriginalRedirect_WhenPresent()
        {
            var context = new CommandContext(item, Interfaces.Editing, CreatePrincipal("admin"), nullBinder, nullValidator);
            context.RedirectTo = "/take/me/back";

            var command = CreateCommand(context);
            dispatcher.Execute(command, context);

            Assert.That(context.RedirectTo, Is.EqualTo("/take/me/back"));
		}

		[Test]
		public void CanMoveItem_ToBefore_Item()
		{
			var child2 = CreateOneItem<StatefulItem>(0, "child2", item);
			var context = new CommandContext(child2, Interfaces.Editing, CreatePrincipal("admin"), nullBinder, nullValidator);
			context.Parameters["MoveBefore"] = child.Path;
			var command = CreateCommand(context);
			dispatcher.Execute(command, context);

			Assert.That(item.Children.Count, Is.EqualTo(2));
			Assert.That(item.Children[0], Is.EqualTo(child2));
			Assert.That(item.Children[1], Is.EqualTo(child));
		}
    }
}