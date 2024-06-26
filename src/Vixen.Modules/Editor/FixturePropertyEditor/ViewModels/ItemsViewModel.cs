﻿using Catel.Data;
using Catel.MVVM;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Catel.IoC;
using Catel.MVVM.Views;
using VixenModules.Editor.FixturePropertyEditor.Views;

namespace VixenModules.Editor.FixturePropertyEditor.ViewModels
{
	/// <summary>
	/// Abstract base class for a for model that maintains a collection of items.
	/// </summary>
	/// <typeparam name="TItemType">Type of item in the collection</typeparam>
	public abstract class ItemsViewModel<TItemType> : FixtureViewModelBase
		where TItemType : ItemViewModel, new()
	{
        #region Constructor 

		/// <summary>
		/// Constructor
		/// </summary>
        public ItemsViewModel()
		{
			// Create the Add button command
			AddCommand = new Command(AddItem, CanAddItem);

			// Create the Delete button commmand
			DeleteCommand = new Command(DeleteItem, CanDeleteItem);

			// Create the collection of items
			Items = new ObservableCollection<TItemType>();

			// Configure Catel to validate immediately
			DeferValidationUntilFirstSaveCall = false;

			// Initialize the validation lock
			_validationLock = new object();
		}

		#endregion

		#region Fields

		/// <summary>
		/// Lock to prevent the Catel validation from producing duplicate validation messages.
		/// </summary>
		private object _validationLock;

		/// <summary>
		/// The data grid for reasons unknown seems to unselect the current row.  This seems to occur more
		/// frequently on a brand new row.  This field is used to save off the selected row so that
		/// delete can be supported 100% of the time. 
		/// </summary>
		private TItemType _backupSelectedItem;

		#endregion

		#region Public Properties

		/// <summary>
		/// Command for adding a new item.
		/// </summary>
		public ICommand AddCommand { get; private set; }
		
		/// <summary>
		/// Command for deleting an item.
		/// </summary>
		public ICommand DeleteCommand { get; private set; }
		
		/// <summary>
		/// Collection of items maintained by the view model.
		/// </summary>
		public ObservableCollection<TItemType> Items { get; set; }

		/// <summary>
		/// Flag that indicates a new item (row) is being added.
		/// </summary>
		public bool AddItemInProgress { get; set; }

		#endregion

		#region Protected Methods

		/// <summary>
		/// Determines if an item can be deleted.
		/// </summary>
		/// <returns>True if an item can be deleted</returns>
		protected virtual bool CanDeleteItem()
		{
			// By default items can deleted
			return true;
		}

		/// <summary>
		/// Determines if an item can be added.
		/// </summary>
		/// <returns>True if an item can be added</returns>
		protected virtual bool CanAddItem()
		{
			// Always allowing the user to create new items
			return true; 
		}

		/// <summary>
		/// For reasons unknown Catel seems to continue to validate items that have been removed from the collection.
		/// This method ensures they are valid so they don't produce errors.
		/// </summary>
		/// <param name="item">Item to make valid</param>
		protected abstract void MakeObjectValidBeforeDeleting(TItemType item);
        
		/// <summary>
		/// Deletes the currently selected item.
		/// </summary>
		protected virtual void DeleteItem()
		{
			// If the selected row is null but the backup selected item is populated then... 
			// This case is expected when only a cell on the current row is selected.
			if (SelectedItem == null && _backupSelectedItem != null)
			{
				// Reset the selected item to the backup
				SelectedItem = _backupSelectedItem;
			}

			// If an item is selected then...
			if (SelectedItem != null)
			{				
				// Make the item being deleted valid so that Catel does not show zombie errors
				MakeObjectValidBeforeDeleting(SelectedItem);

				// Validate the selected item to remove any zombie errors
				SelectedItem.Validate(true);
				
				// Remove the item
				Items.Remove(SelectedItem);
				
				// Clear out the selected item
				SelectedItem = null;

				// Clear out the backup selected item
				_backupSelectedItem = null;

				// Validate the view model
				Validate(true);								
				
				// Get the Catel IViewManager
				IViewManager viewManager = ServiceLocator.Default.ResolveType<IViewManager>();

				// Get the views associated with this view model
				IView[] views = viewManager.GetViewsOfViewModel(this);

				// If the view implements IRefreshGrid
				if (views[0] is IRefreshGrid)
				{
					// Refresh the grid items in the view
					((IRefreshGrid)views[0]).Refresh();
				}

				// Refresh command enable/disable status
				RaiseCanExecuteChanged();
			}
		}

		/// <summary>
		/// Adds a new item.
		/// </summary>
		protected virtual void AddItem()
		{
			// Indicate that a new item is being added
			AddItemInProgress = true;

			// Create the new item
			TItemType item = CreateNewItem();

			// Add the item to the collection
			Items.Add(item);

			// Select the item
			SelectedItem = item;

			// Indicate that we are done adding the new item
			AddItemInProgress = false;

			// Refresh command enable/disable status
			RaiseCanExecuteChanged();
		}

		/// <summary>
		/// Creates a new item.
		/// </summary>
		/// <returns>Newly created item</returns>
		protected virtual TItemType CreateNewItem()
		{
			// Create the new item
			TItemType newItem = new TItemType();
			
			// Give the item the delegate to refresh the command button status
			newItem.RaiseCanExecuteChanged = RaiseCanExecuteChanged;

			// Create the new item
			return newItem;
		}
		
		/// <summary>
		/// Gives derived classes the opportunity to update the view model when an item is selected.
		/// </summary>
		/// <param name="item">Item that was selected</param>
		protected virtual void OnSelectItem(TItemType item)
		{
			// By default do nothing when an item is selected
		}

		#endregion

		#region Public Catel Properties

		/// <summary>
		/// Selected item property.
		/// </summary>
		public TItemType SelectedItem
		{
			get { return GetValue<TItemType>(SelectedItemProperty); }
			set
			{
				// If the newly selected item is NOT null then...
				if (value != null)
				{
					// Update backup of the selected item
					// When selecting a cell sometimes the datagrid is deselecting the item
					_backupSelectedItem = value;
				}

				SetValue(SelectedItemProperty, value);
				
				// If an item was selected then...
				if (value != null)
				{
					// Give the derived classes an opportunity to process the selected item
					OnSelectItem(value);
				}
			}			
		}
	
		/// <summary>
		/// Selected item property data.
		/// </summary>
		public static readonly IPropertyData SelectedItemProperty = RegisterProperty<TItemType>(nameof(SelectedItem));

		#endregion

		#region IFixtureSaveable

		/// <summary>
		/// Refer to interface documentation.
		/// </summary>		
		public override string GetValidationResults()
		{
			// Default the validation result to an empty string
			string validationResults = base.GetValidationResults();	

			// Loop over the items
			foreach (TItemType item in Items)
			{
				// Concatenate the validation results from the color wheel entry
				validationResults += item.GetValidationResults();
			}

			return validationResults;
		}

		/// <summary>
		/// Refer to interface documentation.
		/// </summary>		
		public override bool CanSave()
		{
			// Default to being able to save
			bool canSave = true;

			// Force Catel to validate
			Validate(true);

			// To prevent duplicate validation messaages do not allow more than one thread 
			// in this portion of the method.
			lock (_validationLock)
			{
				// Clear out the previous save validation results
				CanSaveValidationResults = string.Empty;
				
				// Loop over the index items
				foreach (TItemType item in Items)
				{
					// And in the item CanSave status
					canSave &= item.CanSave();
				}

				// Validate the top level fields
				List<IFieldValidationResult> fieldValidationResults = new List<IFieldValidationResult>();
				ValidateFields(fieldValidationResults);

				// Loop over the field validation results
				foreach (IFieldValidationResult result in fieldValidationResults)
				{
					// Concatenate the error messages
					CanSaveValidationResults += result.Message + "\n";

					// Indicate the fixture cannot be saved
					canSave = false;
				}

				// Validate the business
				List<IBusinessRuleValidationResult> validationResults = new List<IBusinessRuleValidationResult>();
				ValidateBusinessRules(validationResults);

				// Loop over the business rule validation results
				for (int index = 0; index < validationResults.Count; index++)
				{
					// Get the specified validation result
					IBusinessRuleValidationResult validationResult = validationResults[index];

					// Concatenate the error messages
					CanSaveValidationResults += validationResult.Message + "\n";

					// Indicate the fixture cannot be saved
					canSave = false;
				}
			}

			// Return whether the items can be saved
			return canSave;
		}

		#endregion		
	}
}
