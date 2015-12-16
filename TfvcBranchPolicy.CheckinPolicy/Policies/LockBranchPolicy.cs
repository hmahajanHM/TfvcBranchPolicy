﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace TfvcBranchPolicy.CheckinPolicy.Common
{
    [Serializable]
    public class LockBranchPolicy : ObservableBase, IBranchPolicy
    {
        [OptionalField(VersionAdded = 2)]
        private Boolean _IsLocked;
        [OptionalField(VersionAdded = 2)]
        private Boolean _IsByPassEnabled;
        [OptionalField(VersionAdded = 2)]
        private string _BypassString;

        public string BypassString
        {
            get
            {
                return _BypassString;
            }
            set
            {
                _BypassString = value;
                OnPropertyChanged("BypassString");
            }
        }
        public Boolean IsLocked
        {
            get
            {
                return _IsLocked;
            }
            set
            {
                _IsLocked = value;
                OnPropertyChanged("IsLocked");
            }
        }

        public Boolean IsByPassEnabled
        {
            get
            {
                return _IsByPassEnabled;
            }
            set
            {
                _IsByPassEnabled = value;
                OnPropertyChanged("IsByPassEnabled");
            }
        }

        public List<BranchPolicyFailure> EvaluatePendingCheckin(BranchPattern branchPattern, Microsoft.TeamFoundation.VersionControl.Client.IPendingCheckin pendingCheckin)
        {
            List<BranchPolicyFailure> branchPolicyFailures = new List<BranchPolicyFailure>();
            if (branchPolicyFailures == null)
            {
                throw new ArgumentNullException("branchPolicyFailures");
            }

            Boolean lockedWithNoBypass = false;
            Boolean lockedWithUnmatchedBypass = false;
            // Evaluate LockPolicy
            if (IsLocked && !IsByPassEnabled)
            {
                lockedWithNoBypass = true;
            }
            else if (IsLocked && IsByPassEnabled)
            {

                if ((pendingCheckin.PendingChanges.Comment != null) && (BypassString != null))
                {
                    if (!pendingCheckin.PendingChanges.Comment.Contains(BypassString))
                    {
                        lockedWithUnmatchedBypass = true;
                    }

                }
                else
                {
                    lockedWithUnmatchedBypass = true;
                }
            }
            else
            {
                //do nothing
            }

            if (lockedWithNoBypass) { branchPolicyFailures.Add(new BranchPolicyFailure(String.Format("The policy for '{0}' failed because the branch is locked. Bypass has not been enabled", branchPattern.Pattern))); }
            if (lockedWithUnmatchedBypass) { branchPolicyFailures.Add(new BranchPolicyFailure(String.Format("The policy for '{0}' failed because the branch is locked. You can bypass with '{1}' in the comment", branchPattern.Pattern, this.BypassString))); }
            return branchPolicyFailures;
        }

        public string Name
        {
            get
            {
                return "Branch Lock";
            }
        }
    }
}