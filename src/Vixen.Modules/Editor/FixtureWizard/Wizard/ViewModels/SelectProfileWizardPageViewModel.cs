﻿namespace VixenModules.Editor.FixtureWizard.Wizard.ViewModels
{
	using Catel.Data;
	using Catel.Fody;
	using Catel.MVVM;
	using Orc.Wizard;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Linq;
	using VixenModules.App.Fixture;
	using VixenModules.App.FixtureSpecificationManager;
	using VixenModules.Editor.FixtureWizard.Wizard.Models;

	/// <summary>
	/// Wizard view model page that selects an existing fixture profile or initiates the creation of a new profile.
	/// </summary>
	[NoWeaving]
    public class SelectProfileWizardPageViewModel : WizardPageViewModelBase<SelectProfileWizardPage>
    {
		#region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="wizardPage">Select profile wizard page model</param>
		public SelectProfileWizardPageViewModel(SelectProfileWizardPage wizardPage) :
            base(wizardPage)
        {            
            // Create the collection of fixture names
            Fixtures = new ObservableCollection<string>();
                        
            // Loop over the existing fixture specifications
            foreach (FixtureSpecification fixture in FixtureSpecificationManager.Instance().FixtureSpecifications)
            {
                // Add the name of the fixture to the collection
                Fixtures.Add(fixture.Name);
            }

            // Default the 'Select Existing Profile' radio button to disabled
            SelectExistingProfileEnabled = false;

            // If the wizard page is configured to selet an existing fixture and
            // there is at least one fixture then...
            if (wizardPage.SelectExistingProfile && Fixtures.Count > 0)
            {
                // Default the radio button to select an existing fixture profile
                SelectExistingProfile = true;

                // Enable the 'Select Existing Profile' radio button
                SelectExistingProfileEnabled = true;
            }            
            // Otherwise if the profile has not already been named then...
            else if (string.IsNullOrEmpty(wizardPage.ProfileName) &&
                     Fixture == null)
            {
                // Otherwise default to creating a new fixture profile
                CreateNewProfile = true;
            }

            // TODO:  Why do I need this?
            RaisePropertyChanged(nameof(SelectedFixture));
        }

		#endregion

		#region Public Properties

		/// <summary>
		/// Collection of available fixture profile names.
		/// </summary>
		public ObservableCollection<string> Fixtures { get; private set; }

        #endregion

        #region Public Catel Properties        

        /// <summary>
        /// Selected fixture name.
        /// </summary>
        [ViewModelToModel]
        public string SelectedFixture
        {
            get 
            { 
                return GetValue<string>(SelectedFixtureProperty); 
            }
            set 
            { 
                SetValue(SelectedFixtureProperty, value);
                
                // If the fixture name is not empty then...
                if (!string.IsNullOrEmpty(value))
                {
                    // Find the fixture profile in the repository and clone it
                    Fixture = FixtureSpecificationManager.Instance().FixtureSpecifications.Single(fxt => fxt.Name == value).CreateInstanceForClone();
                }
                // Otherwise we are creating a new fixture
                else
                {                    
                    // Create a new profile
                    Fixture = new FixtureSpecification();

                    // Initialize the built-in functions
                    Fixture.InitializeBuiltInFunctions();
                }
            }
        }

        /// <summary>
        /// Selected fixture property data.
        /// </summary>
        public static readonly IPropertyData SelectedFixtureProperty = RegisterProperty<string>(nameof(SelectedFixture));

        /// <summary>
        /// Fixture profile being modified by the Wizard.
        /// </summary>
        [ViewModelToModel]
        public FixtureSpecification Fixture
        {
            get
            {
                return GetValue<FixtureSpecification>(FixtureProperty);
            }
            set
            {
                SetValue(FixtureProperty, value);

                // If a fixture specification has been specified then...
                if (value != null)
                {
                    // Copy the fixture meta-data into the view model properties
                    ProfileName = value.Name;
                    Manufacturer = value.Manufacturer;
                    CreatedBy = value.CreatedBy;
                    Revision = value.Revision;
                }
            }
        }

        /// <summary>
        /// Fixture property data.
        /// </summary>
        public static readonly IPropertyData FixtureProperty = RegisterProperty<FixtureSpecification>(nameof(Fixture));

        /// <summary>
        /// Indicates an existing fixture profile is selected.
        /// </summary>
        [ViewModelToModel]
        public bool SelectExistingProfile
        {
            get { return GetValue<bool>(SelectExistingProfileProperty); }
            set 
            { 
                SetValue(SelectExistingProfileProperty, value); 
                
                // If selecting an existing fixture then...
                if (value)
                {
                    // If there is at least one fixture in the respository and
                    // the selected fixture is null then...
                    if (Fixtures.Count > 0 && SelectedFixture == null)
                    {
                        // Default the selection to the first fixture
                        SelectedFixture = Fixtures[0];
                    }
                }
                else
                {                    
                    // Clear out the selected fixture
                    SelectedFixture = null;
                }

            }
        }
        
        /// <summary>
        /// Select existig profile property data.
        /// </summary>
        public static readonly IPropertyData SelectExistingProfileProperty = RegisterProperty<bool>(nameof(SelectExistingProfile));

        /// <summary>
        /// Flag indicating if the wizard is creating a new fixture profile.
        /// </summary>
        [ViewModelToModel]
        public bool CreateNewProfile 
        {
            get 
            { 
                return GetValue<bool>(CreateNewProfileProperty); 
            }
            set 
            { 
                SetValue(CreateNewProfileProperty, value);

                // If creating a new fixture profile then...
                if (value)
                {
                    // Clear out the selected fixture
                    SelectedFixture = null;
                }
            }
        }
       
        public static readonly IPropertyData CreateNewProfileProperty = RegisterProperty<bool>(nameof(CreateNewProfile));

        /// <summary>
        /// Name of the fixture profile.
        /// </summary>
        [ViewModelToModel]
        public string ProfileName
        {
            get
            {
                return GetValue<string>(ProfileNameProperty);
            }
            set
            {
                SetValue(ProfileNameProperty, value);              
            }
        }

        /// <summary>
        /// Profile name property data.
        /// </summary>
        public static readonly IPropertyData ProfileNameProperty = RegisterProperty<string>(nameof(ProfileName));

        /// <summary>
        /// Name of the company that makes the fixture.
        /// </summary>
        [ViewModelToModel]
        public string Manufacturer
        {
            get
            {
                return GetValue<string>(ManufacturerProperty);
            }
            set
            {
                SetValue(ManufacturerProperty, value);
            }
        }

        /// <summary>
        /// Manufacturer property data.
        /// </summary>
        public static readonly IPropertyData ManufacturerProperty = RegisterProperty<string>(nameof(Manufacturer));

        /// <summary>
        /// Name of the user that created the profile.
        /// </summary>
        [ViewModelToModel]
        public string CreatedBy
        {
            get
            {
                return GetValue<string>(CreatedByProperty);
            }
            set
            {
                SetValue(CreatedByProperty, value);
            }
        }

        /// <summary>
        /// CreatedBy property data.
        /// </summary>
        public static readonly IPropertyData CreatedByProperty = RegisterProperty<string>(nameof(CreatedBy));

        /// <summary>
        /// Revision number of the fixture profile.
        /// </summary>
        [ViewModelToModel]
        public string Revision
        {
            get
            {
                return GetValue<string>(RevisionProperty);
            }
            set
            {
                SetValue(RevisionProperty, value);
            }
        }

        /// <summary>
        /// CreatedBy property data.
        /// </summary>
        public static readonly IPropertyData RevisionProperty = RegisterProperty<string>(nameof(Revision));


        /// <summary>
        /// Whether the 'Select Existing Profile' radio button is enabled.
        /// </summary>
        [ViewModelToModel]
        public bool SelectExistingProfileEnabled
        {
	        get
	        {
		        return GetValue<bool>(SelectExistingProfileEnabledProperty);
	        }
	        set
	        {
		        SetValue(SelectExistingProfileEnabledProperty, value);
	        }
        }

        /// <summary>
        /// SelectExistingProfile property data.
        /// </summary>
        public static readonly IPropertyData SelectExistingProfileEnabledProperty = RegisterProperty<bool>(nameof(SelectExistingProfileEnabled));

        #endregion

        #region Protected Methods

        /// <summary>
        /// Displays the validation bar if there are messages in the validation results.
        /// </summary>
        /// <param name="validationResults">Validation results to examine</param>
        protected void DisplayValidationBar(List<IFieldValidationResult> validationResults)
        {
            // If there are any validation results then...
            if (validationResults.Any())
            {
                // Make sure the validation bar is displayed
                HideValidationResults = false;
            }
        }
        
        /// <summary>
        /// Validates the editable fields.
        /// </summary>
        /// <param name="validationResults">Results of the validation</param>
        protected override void ValidateFields(List<IFieldValidationResult> validationResults)
        {
            // If the profile name is empty then...
            if (string.IsNullOrEmpty(ProfileName))
            {
                validationResults.Add(FieldValidationResult.CreateError(ProfileNameProperty, "Profile Name is empty.  Profile Name is a required field."));
            }

            // Display the validation bar
            DisplayValidationBar(validationResults);
        }

        #endregion
    }
}
