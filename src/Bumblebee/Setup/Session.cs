﻿using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using Bumblebee.Extensions;
using Bumblebee.Interfaces;

using OpenQA.Selenium;

namespace Bumblebee.Setup
{
	public class Session
	{
		public readonly ISettings Settings;

		public IWebDriver Driver { get; private set; }

		public IMonkey Monkey { get; protected set; }

		public Session(IDriverEnvironment environment)
			: this(environment, new Settings())
		{
		}

		public Session(IDriverEnvironment environment, ISettings settings)
		{
			Settings = settings;
			Driver = environment.CreateWebDriver();
		}

		public TBlock NavigateTo<TBlock>(string url) where TBlock : IBlock
		{
			Driver.Navigate().GoToUrl(url);
			return CurrentBlock<TBlock>();
		}

		public TBlock CurrentBlock<TBlock>(IWebElement tag = null) where TBlock : IBlock
		{
			var type = typeof (TBlock);
			IList<Type> constructorSignature = new List<Type> { typeof (Session) };
			IList<object> constructorArgs = new List<object> { this };

			if (typeof (ISpecificBlock).IsAssignableFrom(typeof (TBlock)))
			{
				constructorSignature.Add(typeof (IWebElement));
				constructorArgs.Add(tag);
			}

			var constructor = type.GetConstructor(constructorSignature.ToArray());

			if (constructor == null)
			{
				throw new ArgumentException(String.Format("The result type specified ({0}) is not a valid block. It must have a constructor that takes only a session.", type));
			}

			return (TBlock) constructor.Invoke(constructorArgs.ToArray());
		}

		public void End()
		{
			if (Driver != null)
			{
				Driver.Quit();

				Driver.Dispose();

				Driver = null;
			}
		}

		public Session CaptureScreen()
		{
			var path = Path.ChangeExtension(Path.Combine(Settings.ScreenCapturePath, this.GetParentMethodName()), "png");
			return CaptureScreen(path);
		}

		public Session CaptureScreen(string path)
		{
			var screenshot = ((ITakesScreenshot) Driver).GetScreenshot();
			screenshot.SaveAsFile(path, ImageFormat.Png);
			return this;
		}
	}

	public class Session<TDriverEnvironment> : Session
		where TDriverEnvironment : IDriverEnvironment, new()
	{
		public Session()
			: base(new TDriverEnvironment())
		{
		}

		public Session(ISettings settings)
			: base(new TDriverEnvironment(), settings)
		{
			
		}
	}
}
