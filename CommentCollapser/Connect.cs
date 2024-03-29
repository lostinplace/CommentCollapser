﻿using System;
using Extensibility;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.CommandBars;
using System.Resources;
using System.Reflection;
using System.Data.Linq;

using System.Globalization;

namespace CommentCollapser
{
	/// <summary>The object for implementing an Add-in.</summary>
	/// <seealso class='IDTExtensibility2' />
	public class Connect : IDTExtensibility2, IDTCommandTarget
	{
		/// <summary>Implements the constructor for the Add-in object. Place your initialization code within this method.</summary>
		public Connect()
		{
		}

    private static Command tmpCommand=null;
		/// <summary>Implements the OnConnection method of the IDTExtensibility2 interface. Receives notification that the Add-in is being loaded.</summary>
		/// <param term='application'>Root object of the host application.</param>
		/// <param term='connectMode'>Describes how the Add-in is being loaded.</param>
		/// <param term='addInInst'>Object representing this Add-in.</param>
		/// <seealso class='IDTExtensibility2' />
		public void OnConnection(object application, ext_ConnectMode connectMode, object addInInst, ref Array custom)
		{
			_applicationObject = (DTE2)application;
			_addInInstance = (AddIn)addInInst;
      if (connectMode == ext_ConnectMode.ext_cm_UISetup || connectMode== ext_ConnectMode.ext_cm_Startup)
			{
				object []contextGUIDS = new object[] { };
				Commands2 commands = (Commands2)_applicationObject.Commands;
        string editMenuName = "Edit";

				//Place the command on the tools menu.
				//Find the MenuBar command bar, which is the top-level command bar holding all the main menu items:
				Microsoft.VisualStudio.CommandBars.CommandBar menuBarCommandBar = ((Microsoft.VisualStudio.CommandBars.CommandBars)_applicationObject.CommandBars)["MenuBar"];

				//Find the Tools command bar on the MenuBar command bar:
        CommandBarControl editControl = menuBarCommandBar.Controls[editMenuName];
        CommandBarPopup toolsPopup = (CommandBarPopup)editControl;
        CommandBar tmpBar= ((CommandBars)_applicationObject.CommandBars)["&Outlining"];
				//This try/catch block can be duplicated if you wish to add multiple commands to be handled by your Add-in,
				//  just make sure you also update the QueryStatus/Exec method to include the new command names.
				
        try
				{
					//Add a command to the Commands collection:
          Command command;
          if(tmpCommand==null)
					  tmpCommand = commands.AddNamedCommand2(_addInInstance, "CommentCollapser", "Collapse XML Comments", "Collapses outlining for all entities then toggles expansion on headers", true, 59, ref contextGUIDS, (int)vsCommandStatus.vsCommandStatusSupported+(int)vsCommandStatus.vsCommandStatusEnabled, (int)vsCommandStyle.vsCommandStyleText, vsCommandControlType.vsCommandControlTypeButton);
          command = tmpCommand;
          object[] BindingArray = new object[]{
            "Text Editor::Ctrl+M, Ctrl+K"
          };
          command.Bindings = BindingArray;
					//Add a control for the command to the tools menu:
          try
          {
            if ((command != null) && (toolsPopup != null) && tmpBar != null)
            {
              command.AddControl(tmpBar, 1);
            }
          }
          catch (Exception)
          {
            throw;
          }

				}
				catch(System.ArgumentException)
				{
					//If we are here, then the exception is probably because a command with that name
					//  already exists. If so there is no need to recreate the command and we can 
                    //  safely ignore the exception.
				}
			}
		}

		/// <summary>Implements the OnDisconnection method of the IDTExtensibility2 interface. Receives notification that the Add-in is being unloaded.</summary>
		/// <param term='disconnectMode'>Describes how the Add-in is being unloaded.</param>
		/// <param term='custom'>Array of parameters that are host application specific.</param>
		/// <seealso class='IDTExtensibility2' />
		public void OnDisconnection(ext_DisconnectMode disconnectMode, ref Array custom)
		{
		}

		/// <summary>Implements the OnAddInsUpdate method of the IDTExtensibility2 interface. Receives notification when the collection of Add-ins has changed.</summary>
		/// <param term='custom'>Array of parameters that are host application specific.</param>
		/// <seealso class='IDTExtensibility2' />		
		public void OnAddInsUpdate(ref Array custom)
		{
		}

		/// <summary>Implements the OnStartupComplete method of the IDTExtensibility2 interface. Receives notification that the host application has completed loading.</summary>
		/// <param term='custom'>Array of parameters that are host application specific.</param>
		/// <seealso class='IDTExtensibility2' />
		public void OnStartupComplete(ref Array custom)
		{
		}

		/// <summary>Implements the OnBeginShutdown method of the IDTExtensibility2 interface. Receives notification that the host application is being unloaded.</summary>
		/// <param term='custom'>Array of parameters that are host application specific.</param>
		/// <seealso class='IDTExtensibility2' />
		public void OnBeginShutdown(ref Array custom)
		{
		}
		
		/// <summary>Implements the QueryStatus method of the IDTCommandTarget interface. This is called when the command's availability is updated</summary>
		/// <param term='commandName'>The name of the command to determine state for.</param>
		/// <param term='neededText'>Text that is needed for the command.</param>
		/// <param term='status'>The state of the command in the user interface.</param>
		/// <param term='commandText'>Text requested by the neededText parameter.</param>
		/// <seealso class='Exec' />
		public void QueryStatus(string commandName, vsCommandStatusTextWanted neededText, ref vsCommandStatus status, ref object commandText)
		{
			if(neededText == vsCommandStatusTextWanted.vsCommandStatusTextWantedNone)
			{
				if(commandName == "CommentCollapser.Connect.CommentCollapser")
				{
					status = (vsCommandStatus)vsCommandStatus.vsCommandStatusSupported|vsCommandStatus.vsCommandStatusEnabled;
					return;
				}
			}
		}

		/// <summary>Implements the Exec method of the IDTCommandTarget interface. This is called when the command is invoked.</summary>
		/// <param term='commandName'>The name of the command to execute.</param>
		/// <param term='executeOption'>Describes how the command should be run.</param>
		/// <param term='varIn'>Parameters passed from the caller to the command handler.</param>
		/// <param term='varOut'>Parameters passed from the command handler to the caller.</param>
		/// <param term='handled'>Informs the caller if the command was handled or not.</param>
		/// <seealso class='Exec' />
		public void Exec(string commandName, vsCommandExecOption executeOption, ref object varIn, ref object varOut, ref bool handled)
		{
			handled = false;
			if(executeOption == vsCommandExecOption.vsCommandExecOptionDoDefault)
			{
				if(commandName == "CommentCollapser.Connect.CommentCollapser")
				{
          Document aDoc = _applicationObject.ActiveDocument;

          //inspect undo context before begin
          bool fUndoWasOpen = _applicationObject.UndoContext.IsOpen;
          if (!fUndoWasOpen)
            _applicationObject.UndoContext.Open("Collapse XML Comments");
          
          _applicationObject.ExecuteCommand("Edit.CollapseToDefinitions");
          
          foreach (CodeElement item in aDoc.ProjectItem.FileCodeModel.CodeElements)
          {
            TextPoint itemStart;
            TextSelection selection;
            try
            {
              //ugh
              itemStart = item.GetStartPoint(vsCMPart.vsCMPartWholeWithAttributes);
              ((TextSelection)aDoc.Selection).MoveToPoint(itemStart);
              _applicationObject.ExecuteCommand("Edit.ToggleOutliningExpansion");

              ProsecuteElement(item, delegate(CodeElement anItem)
              {
                try
                {
                  //oddly, header does not includ attributes/xdcoc comments... weird
                  itemStart = anItem.GetStartPoint(vsCMPart.vsCMPartHeader);
                  selection = ((TextSelection)aDoc.Selection);

                  //using the line instead of pointbecause it makes it easier to view in debug
                  selection.GotoLine(itemStart.Line);
                  _applicationObject.ExecuteCommand("Edit.ToggleOutliningExpansion");
                }
                catch (Exception ey)
                {
                  var c = ey;
                }
                
              });
            }
            catch (Exception ex)
            {
              var b = ex;
            }
          }
          _applicationObject.UndoContext.Close();
					handled = true;
					return;
				}
			}
		}

    /// <summary>
    /// Recursively iterates through all of the children
    /// </summary>
    /// <param name="anElement">origin parent element</param>
    /// <param name="anAction">action to commit on each of the children before recursing</param>
    public static void ProsecuteElement(CodeElement anElement, Action<CodeElement> anAction)
    {
      foreach (CodeElement item in anElement.Children)
      {
        anAction(item);
        ProsecuteElement(item,anAction);
      }
    }

		private DTE2 _applicationObject;
		private AddIn _addInInstance;
	}
}