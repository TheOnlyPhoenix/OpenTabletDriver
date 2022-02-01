using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json.Serialization;

namespace OpenTabletDriver.Desktop.Profiles
{
    public class ProfileCollection : ObservableCollection<Profile>
    {
        private readonly IServiceProvider _serviceProvider;

        [JsonConstructor]
        private ProfileCollection()
        {
        }

        public ProfileCollection(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public ProfileCollection(IServiceProvider serviceProvider, IEnumerable<Profile> profiles)
            : base(profiles)
        {
            this._serviceProvider = serviceProvider;
        }

        public ProfileCollection(IServiceProvider serviceProvider, IEnumerable<InputDevice> devices)
            : this(serviceProvider, devices.Select(s => Profile.GetDefaults(serviceProvider, s)))
        {
        }

        public Profile this[InputDevice tablet]
        {
            set => SetProfile(tablet, value);
            get => GetProfile(tablet);
        }

        public void SetProfile(InputDevice tablet, Profile profile)
        {
            if (this.FirstOrDefault(t => t.Tablet == tablet.Properties.Name) is Profile oldProfile)
            {
                Remove(oldProfile);
            }
            Add(profile);
        }

        public Profile GetProfile(InputDevice tablet)
        {
            return this.FirstOrDefault(t => t.Tablet == tablet.Properties.Name) is Profile profile
                ? profile : Generate(tablet);
        }

        public Profile Generate(InputDevice tablet)
        {
            var profile = Profile.GetDefaults(_serviceProvider, tablet);
            SetProfile(tablet, profile);
            return profile;
        }
    }
}
