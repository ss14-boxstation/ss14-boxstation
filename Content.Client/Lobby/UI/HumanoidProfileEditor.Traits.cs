using Content.Client.Lobby.UI.Roles;
using Content.Client.Stylesheets;
using Content.Shared._DV.Traits;
using Content.Shared.Traits;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using System.Linq;

namespace Content.Client.Lobby.UI;

public sealed partial class HumanoidProfileEditor
{
    /// <summary>
    /// Refreshes traits selector
    /// </summary>
    // Start Box Change: DeltaV trait system
    // public void RefreshTraits()
    // {
    //     TraitsList.RemoveAllChildren();
    //
    //     var traits = _prototypeManager.EnumeratePrototypes<TraitPrototype>().OrderBy(t => Loc.GetString(t.Name)).ToList();
    //     TabContainer.SetTabTitle(3, Loc.GetString("humanoid-profile-editor-traits-tab"));
    //
    //     if (traits.Count < 1)
    //     {
    //         TraitsList.AddChild(new Label
    //         {
    //             Text = Loc.GetString("humanoid-profile-editor-no-traits"),
    //             FontColorOverride = Color.Gray,
    //         });
    //         return;
    //     }
    //
    //     // Setup model
    //     Dictionary<string, List<string>> traitGroups = new();
    //     List<string> defaultTraits = new();
    //     traitGroups.Add(TraitCategoryPrototype.Default, defaultTraits);
    //
    //     foreach (var trait in traits)
    //     {
    //         // Begin DeltaV Additions - Species trait exclusion
    //         if (Profile?.Species is { } selectedSpecies && trait.ExcludedSpecies.Contains(selectedSpecies))
    //         {
    //             Profile = Profile?.WithoutTraitPreference(trait.ID, _prototypeManager);
    //             continue;
    //         }
    //         // End DeltaV Additions
    //
    //         if (trait.Category == null)
    //         {
    //             defaultTraits.Add(trait.ID);
    //             continue;
    //         }
    //
    //         if (!_prototypeManager.HasIndex(trait.Category))
    //             continue;
    //
    //         var group = traitGroups.GetOrNew(trait.Category);
    //         group.Add(trait.ID);
    //     }
    //
    //     // Create UI view from model
    //     foreach (var (categoryId, categoryTraits) in traitGroups)
    //     {
    //         TraitCategoryPrototype? category = null;
    //
    //         if (categoryId != TraitCategoryPrototype.Default)
    //         {
    //             category = _prototypeManager.Index<TraitCategoryPrototype>(categoryId);
    //             // Label
    //             TraitsList.AddChild(new Label
    //             {
    //                 Text = Loc.GetString(category.Name),
    //                 Margin = new Thickness(0, 10, 0, 0),
    //                 StyleClasses = { StyleClass.LabelHeading },
    //             });
    //         }
    //
    //         List<TraitPreferenceSelector?> selectors = new();
    //         var selectionCount = 0;
    //
    //         foreach (var traitProto in categoryTraits)
    //         {
    //             var trait = _prototypeManager.Index<TraitPrototype>(traitProto);
    //             var selector = new TraitPreferenceSelector(trait);
    //
    //             selector.Preference = Profile?.TraitPreferences.Contains(trait.ID) == true;
    //             if (selector.Preference)
    //                 selectionCount += trait.Cost;
    //
    //             selector.PreferenceChanged += preference =>
    //             {
    //                 if (preference)
    //                 {
    //                     Profile = Profile?.WithTraitPreference(trait.ID, _prototypeManager);
    //                 }
    //                 else
    //                 {
    //                     Profile = Profile?.WithoutTraitPreference(trait.ID, _prototypeManager);
    //                 }
    //
    //                 SetDirty();
    //                 RefreshTraits(); // If too many traits are selected, they will be reset to the real value.
    //             };
    //             selectors.Add(selector);
    //         }
    //
    //         // Selection counter
    //         if (category is { MaxTraitPoints: >= 0 })
    //         {
    //             TraitsList.AddChild(new Label
    //             {
    //                 Text = Loc.GetString("humanoid-profile-editor-trait-count-hint", ("current", selectionCount) ,("max", category.MaxTraitPoints)),
    //                 FontColorOverride = Color.Gray
    //             });
    //         }
    //
    //         foreach (var selector in selectors)
    //         {
    //             if (selector == null)
    //                 continue;
    //
    //             if (category is { MaxTraitPoints: >= 0 } &&
    //                 selector.Cost + selectionCount > category.MaxTraitPoints)
    //             {
    //                 selector.Checkbox.Label.FontColorOverride = Color.Red;
    //             }
    //
    //             TraitsList.AddChild(selector);
    //         }
    //     }
    // }

    /// <summary>
    /// Updates the traits tab with the current profile's selected traits.
    /// </summary>
    private void UpdateTraitsSelection()
    {
        if (Profile is null)
        {
            Traits.SetSelectedTraits(new HashSet<ProtoId<TraitPrototype>>());
            return;
        }

        // Convert profile's trait preferences (strings) to ProtoId<TraitPrototype>
        var selectedTraits = new HashSet<ProtoId<TraitPrototype>>(Profile.TraitPreferences.Count);
        foreach (var traitId in Profile.TraitPreferences)
        {
            // Validate that the trait still exists in prototypes
            if (_prototypeManager.HasIndex(traitId))
            {
                selectedTraits.Add(new ProtoId<TraitPrototype>(traitId));
            }
        }

        Traits.ForcePassAllConditions(); // Box Change:
                                         // Force all traits to be selectable before setting them, to avoid weirdness with outdated restrictions.
                                         // e.g. when switching from a human to a non-human with the red blood trait, the red blood trait would be de-selected since it's still considered locked.
                                         // Traits that are actually invalid after setup is done will be disabled by UpdateConditions().
        Traits.SetSelectedTraits(selectedTraits);
        Traits.UpdateConditions(Profile);
    }
    // End Box Change

}
