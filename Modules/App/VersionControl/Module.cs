﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NGit.Api;
using Vixen.Module;
using Vixen.Module.App;
using Vixen.Sys;

namespace VersionControl
{
    public partial class Module : AppModuleInstanceBase
    {
        public Module()
        {
            _data = new Data();

        }
        private static NLog.Logger Logging = NLog.LogManager.GetCurrentClassLogger();
		
        #region Variables
        IApplication _application;
        Data _data;
        #endregion

        #region overrides

        public override IModuleDataModel StaticModuleData
        {
            get
            {

                return _data;
            }
            set
            {
                _data = (Data)value;
            }
        }

        public override void Loading()
        {

            _AddApplicationMenu();


            EnableDisableSourceControl(_data.IsEnabled);


        }

        public override void Unloading()
        {
            DisableWatchers();
        }

        public override Vixen.Sys.IApplication Application
        {
            set { _application = value; }
        }


        #endregion

        #region Application Menu
        private const string MENU_ID_ROOT = "VersionControlRoot";

        private AppCommand _showCommand;
        private LatchedAppCommand _enabledCommand;

        private void _AddApplicationMenu()
        {
            if (_AppSupportsCommands())
            {
                var toolsMenu = _GetToolsMenu();
                var rootCommand = new AppCommand(MENU_ID_ROOT, "Version Control");
                rootCommand.Add(_enabledCommand ?? (_enabledCommand = _CreateEnabledCommand()));
                rootCommand.Add(new AppCommand("s1", "-"));
                rootCommand.Add(_showCommand ?? (_showCommand = _CreateShowCommand()));

                toolsMenu.Add(rootCommand);
            }
        }

        private AppCommand _CreateShowCommand()
        {
            AppCommand showCommand = new AppCommand("VersionControl", "Browse");
            showCommand.Click += (sender, e) =>
            {
                using (Versioning cs = new Versioning((Data)StaticModuleData, repo))
                {

                    if (cs.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {


                    }
                }
            };

            return showCommand;
        }
        private LatchedAppCommand _CreateEnabledCommand()
        {
            LatchedAppCommand enabledCommand = new LatchedAppCommand("VersionControlEnabled", "Enabled");
            enabledCommand.IsChecked = _data.IsEnabled;
            enabledCommand.Checked += async (sender, e) =>
            {
                _data.IsEnabled = e.CheckedState;
                EnableDisableSourceControl(_data.IsEnabled);
	            await VixenSystem.SaveModuleConfigAsync();
			};

            return enabledCommand;
        }


        private void _RemoveApplicationMenu()
        {
            if (_AppSupportsCommands())
            {
                AppCommand toolsMenu = _GetToolsMenu();
                toolsMenu.Remove(MENU_ID_ROOT);
            }
        }
        private bool _AppSupportsCommands()
        {
            return _application != null && _application.AppCommands != null;
        }
        private AppCommand _GetToolsMenu()
        {
            AppCommand toolsMenu = _application.AppCommands.Find("Tools");
            if (toolsMenu == null)
            {
                toolsMenu = new AppCommand("Tools", "Tools");
                _application.AppCommands.Add(toolsMenu);
            }
            return toolsMenu;
        }


        #endregion

        #region File System Watchers
        private void CreateWatcher(string folder)
        {
			Directory.GetDirectories(folder).Where(d => !d.EndsWith("logs") && !d.EndsWith("\\.git") && !d.EndsWith("Core Logs") && !d.EndsWith("Export")).ToList().ForEach(CreateDirectoryWatcher);
        }

	    private void CreateDirectoryWatcher(string folder)
	    {
		    var watcher = new FileSystemWatcher(folder);
		    watcher.Changed += watcher_FileSystemChanges;
		    watcher.Created += watcher_FileSystemChanges;
		    watcher.Deleted += watcher_FileSystemChanges;
		    watcher.Renamed += watcher_FileSystemChanges;
		    watcher.IncludeSubdirectories = true;
		    watcher.EnableRaisingEvents = true;
		    watchers.Add(watcher);
	    }

	    private void watcher_FileSystemChanges(object sender, FileSystemEventArgs e)
        {
			Task.Factory.StartNew(() =>
            {
                try
                {
					lock (fileLockObject)
					{
						//Wait for the file to fully save...
						Thread.Sleep(1000);
						while (VixenSystem.IsSaving())
						{
							Thread.Sleep(1);
						}
						Git git = new Git(repo);

						var status = git.Status().Call();

						var changed = status.GetAdded().Count > 0 || status.GetChanged().Count > 0 || status.GetModified().Count > 0
						              || status.GetRemoved().Count > 0 || status.GetUntracked().Count > 0 || status.GetMissing().Count > 0;

						if (status.GetAdded().Count > 0 || status.GetUntracked().Count > 0 || status.GetModified().Count > 0
							|| status.GetChanged().Count > 0)
						{
							var add = git.Add();
							status.GetAdded().ToList().ForEach(a =>
							{
								add.AddFilepattern(a);
							});

							status.GetModified().ToList().ForEach(a =>
							{
								add.AddFilepattern(a);
							});

							status.GetChanged().ToList().ForEach(a =>
							{
								add.AddFilepattern(a);
							});

							status.GetUntracked().ToList().ForEach(a =>
							{
								add.AddFilepattern(a);
							});

							add.Call();
						}


						if (status.GetMissing().Count > 0 || status.GetRemoved().Count > 0)
						{
							var removed = git.Rm();

							status.GetRemoved().ToList().ForEach(a =>
							{
								removed.AddFilepattern(a);
							});

							status.GetMissing().ToList().ForEach(a =>
							{
								removed.AddFilepattern(a);
							});

							removed.Call();
						}

						if(changed)
						{
							git.Commit().SetMessage(string.Format("Changes to the profile {0}",
							restoringFile ? "restored." : "commited.")).Call();
						}

						//switch (e.ChangeType)
						//{

						//	case WatcherChangeTypes.Changed:
						//		if (status.GetModified().Count + status.GetChanged().Count +
						//				status.GetChanged().Count > 0)
						//		{
						//			var addChanged = git.Add();
						//			foreach (var s in status.GetModified())
						//			{
						//				addChanged.AddFilepattern(s);
						//			}
						//			foreach (var s in status.GetChanged())
						//			{
						//				addChanged.AddFilepattern(s);
						//			}
						//			addChanged.Call();
						//			git.Commit().SetMessage(string.Format("Changed {0}{1}", e.Name,
						//				restoringFile ? " [Restored]" : "")).Call();
						//		}
						//		break;
						//	case WatcherChangeTypes.Created:

						//		if (status.GetUntracked().Count + status.GetAdded().Count > 0)
						//		{
						//			var addCreated = git.Add();
						//			foreach(var s in status.GetAdded())
						//			{
						//				addCreated.AddFilepattern(s);
						//			}
						//			foreach (var s in status.GetUntracked())
						//			{
						//				addCreated.AddFilepattern(s);
						//			}

						//			addCreated.Call();
						//			git.Commit().SetMessage(string.Format("Added {0}{1}", e.Name,
						//				restoringFile ? " [Restored]" : "")).Call();
						//		}
						//		break;
						//	case WatcherChangeTypes.Deleted:

						//		if ((status.GetMissing().Count + status.GetRemoved().Count) > 0)
						//		{
						//			var removed = git.Rm();
						//			foreach (var s in status.GetMissing())
						//			{
						//				removed.AddFilepattern(s);
						//			}
						//			foreach (var s in status.GetRemoved())
						//			{
						//				removed.AddFilepattern(s);
						//			}
						//			removed.Call();
						//			git.Commit().SetMessage(string.Format("Deleted {0}{1}", e.Name,
						//				restoringFile ? " [Restored]" : "")).Call();
						//		}
						//		break;
						//	case WatcherChangeTypes.Renamed:

						//		if (status.GetMissing().Count + status.GetUntracked().Count > 0)
						//		{
						//			var addRename = git.Add();
						//			foreach (var s in status.GetMissing())
						//			{
						//				addRename.AddFilepattern(s);
						//			}
						//			foreach (var s in status.GetUntracked())
						//			{
						//				addRename.AddFilepattern(s);
						//			}
						//			addRename.Call();
						//			git.Commit().SetMessage(string.Format("Renamed file {0} to {1}{2}",
						//				((RenamedEventArgs)e).OldName, e.Name,
						//				restoringFile ? " [Restored]" : "")).Call();
						//		}
						//		break;
						//}
					}
				}
                catch (Exception eee)
                {

                    Logging.Error(eee.Message,eee);
                }

                restoringFile = false;
                //Reset the cache when GIT changes
                Versioning.GitDetails = null;


            });
        }


        private void DisableWatchers()
        {
            watchers.ForEach(w => w.EnableRaisingEvents = false);
            watchers.Clear();
        }



        internal static bool restoringFile = false;
        internal static object fileLockObject = new object();
        List<FileSystemWatcher> watchers = new List<FileSystemWatcher>();

        #endregion



    }
}
