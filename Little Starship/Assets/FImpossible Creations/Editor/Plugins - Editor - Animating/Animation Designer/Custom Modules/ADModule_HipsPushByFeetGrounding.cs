using FIMSpace.FEditor;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static FIMSpace.AnimationTools.ADClipSettings_CustomModules;

namespace FIMSpace.AnimationTools.CustomModules
{

    public class ADModule_HipsPushByFeetGrounding : ADCustomModuleBase
    {
        public override string ModuleTitleName { get { return "Inverse Kinematics (IK)/Hips Push With Feet Grounding"; } }
        public override bool GUIFoldable { get { return false; } }
        public override bool SupportBlending { get { return true; } }

        bool resetHips = true;
        public override void OnResetState( CustomModuleSet customModuleSet )
        {
            base.OnResetState( customModuleSet );
            resetHips = true;
        }

        Vector3 _sd_hipsSmooth = Vector3.zero;
        Vector3 _hipsSmooth = Vector3.zero;

        public override void OnInfluenceIKUpdate( float animationProgress, float deltaTime, AnimationDesignerSave s, ADClipSettings_Main anim_MainSet, ADClipSettings_CustomModules customModules, ADClipSettings_CustomModules.CustomModuleSet set )
        {
            var ikSet = s.GetSetupForClip<ADClipSettings_IK>( s.IKSetupsForClips, anim_MainSet.settingsForClip, AnimationDesignerWindow._toSet_SetSwitchToHash );
            if( ikSet == null ) return;
            if( anim_MainSet.Pelvis == null ) return;

            if( resetHips )
            {
                _hipsSmooth = Vector3.zero;
                _sd_hipsSmooth = Vector3.zero;
                resetHips = false;
            }

            Vector3 offset = Vector3.zero;

            var pDownPower = GVar( "pDown", "Push Down Power:", 0.1f );
            pDownPower.HideFlag = true;
            float downPower = pDownPower.Float;

            var lBack = GVar( "lBack", "Look Back:", 0.25f );
            float backProgress = animationProgress - ( lBack.Float * ( 1f / anim_MainSet.settingsForClip.length ) );

            for( int l = 0; l < legs.Count; l++ )
            {
                var limb = s.Limbs[legs[l].Index];
                var ikSettings = ikSet.GetIKSettingsForLimb( limb, s );
                if( ikSettings == null ) continue;

                bool wasGrounded = ikSettings.FootDataAnalyze.GroundedIn( backProgress );
                bool isGrounded = ikSettings.FootDataAnalyze.GroundedIn( animationProgress );

                if( wasGrounded == false && isGrounded )
                {
                    offset += new Vector3( 0f, -downPower, 0f );
                }
            }

            var smoothingMot = GVar( "SM", "Smoothing Motion:", 0.3f );
            smoothingMot.HideFlag = true;
            smoothingMot.SetRangeHelperValue( 0f, 1f );

            float smoothingOffset = smoothingMot.Float;

            if( smoothingOffset <= 0f )
                _hipsSmooth = offset;
            else
                _hipsSmooth = Vector3.SmoothDamp( _hipsSmooth, offset, ref _sd_hipsSmooth, 0.001f + 0.499f * smoothingOffset, float.MaxValue, deltaTime );

            //_hipsSmooth = middlePelvisPos;
            anim_MainSet.PelvisFrameCustomPositionOffset += _hipsSmooth * GetEvaluatedBlend( set, animationProgress );
        }


        #region Editor GUI Related Code

        [HideInInspector] public bool _InitialInfoClicked = false;
        List<ADClipSettings_IK.IKSet> legs = new List<ADClipSettings_IK.IKSet>();

        void PrepareReferences( AnimationDesignerSave s, ADClipSettings_Main _anim_MainSet )
        {
            currentSave = s;
            currentMainSet = _anim_MainSet;

            var ikSet = s.GetSetupForClip<ADClipSettings_IK>( s.IKSetupsForClips, _anim_MainSet.settingsForClip, _anim_MainSet.SetIDHash );
            if( ikSet == null ) return;

            if( ikSet.LimbIKSetups.Count == 0 )
            {
                return;
            }

            if( legs == null ) legs = new List<ADClipSettings_IK.IKSet>();
            if( legs.Count > 0 ) if( legs[0] != ikSet.LimbIKSetups[0] ) legs.Clear();
            if( legs.Count <= 0 || legs[0] == null || legs[0].LastUsedProcessor == null )
            {
                legs.Clear();
                for( int i = 0; i < ikSet.LimbIKSetups.Count; i++ )
                {
                    if( ikSet.LimbIKSetups[i].IKType == ADClipSettings_IK.IKSet.EIKType.FootIK )
                    {
                        legs.Add( ikSet.LimbIKSetups[i] );
                    }
                }
            }
        }


        AnimationDesignerSave currentSave = null;
        ADClipSettings_Main currentMainSet = null;

        public override void InspectorGUI_ModuleBody( float optionalBlendGhost, ADClipSettings_Main _anim_MainSet, AnimationDesignerSave s, ADClipSettings_CustomModules cModule, CustomModuleSet set )
        {
            currentSave = s;
            currentMainSet = _anim_MainSet;

            #region Prepare Resources

            var ikSet = s.GetSetupForClip<ADClipSettings_IK>( s.IKSetupsForClips, _anim_MainSet.settingsForClip, AnimationDesignerWindow._toSet_SetSwitchToHash );

            if( ikSet == null )
            {
                EditorGUILayout.HelpBox( "Can't Find IK Set!", MessageType.Warning );
                return;
            }

            #endregion

            #region Legs IK Search

            if( ikSet.LimbIKSetups.Count == 0 )
            {
                EditorGUILayout.HelpBox( "Can't Find Limb IK Setups!", MessageType.Warning );
                return;
            }

            PrepareReferences( s, _anim_MainSet );

            if( legs.Count <= 0 )
            {
                EditorGUILayout.HelpBox( "Can't Find Limb IK Legs!", MessageType.Warning );
                return;
            }

            if( _anim_MainSet.TurnOnIK == false )
            {
                EditorGUILayout.HelpBox( "Go to IK Tab and enable using IK", MessageType.Warning );
                return;
            }

            if( legs[0].Enabled == false )
            {
                EditorGUILayout.HelpBox( "Some legs IK seems to be disabled! This module will work only with enabled foot IKs!", MessageType.Info );
                if( GUILayout.Button( "Enable Foot IKs" ) )
                {
                    for( int i = 0; i < legs.Count; i++ ) legs[i].Enabled = true;
                    S._SetDirty();
                }
            }
            else
            {
                if( !_InitialInfoClicked )
                {
                    EditorGUILayout.HelpBox( "Pushing pelvis down when detected current foot height at zero value and previous (look back seconds) height above 0", MessageType.None );
                    var r = GUILayoutUtility.GetLastRect();

                    if( GUI.Button( r, GUIContent.none, EditorStyles.label ) )
                    {
                        _InitialInfoClicked = true;
                    }

                    GUILayout.Space( 6 );
                }
            }

            #endregion

            bool wasAnalyzed = true;
            for( int l = 0; l < legs.Count; l++ )
            {
                var ikSettings = ikSet.GetIKSettingsForLimb( s.Limbs[legs[l].Index], s );
                if( ikSettings == null || ikSettings.WasAnalyzed == false ) { wasAnalyzed = false; break; }
            }

            if( wasAnalyzed == false )
            {
                if( GUILayout.Button( "Some leg ik data was not analyzed, hit to do it.\n(go to IK -> Foot IK Mode -> Grounding for more custom settings)" ) )
                {
                    for( int l = 0; l < legs.Count; l++ )
                    {
                        var limb = s.Limbs[legs[l].Index];
                        var ikSettings = ikSet.GetIKSettingsForLimb( limb, s );
                        if( ikSettings != null ) ikSettings.GetClipFootAnalyze( S, limb, _anim_MainSet.settingsForClip, _anim_MainSet, ikSettings.HeelGroundedTreshold, ikSettings.ToesGroundedTreshold, ikSettings.HoldOnGround, _anim_MainSet.ResetRootPosition );
                    }
                }
            }
            else
            {
                var firstLegSettings = ikSet.GetIKSettingsForLimb( s.Limbs[legs[0].Index], s );

                for( int l = 0; l < legs.Count; l++ )
                {
                    var limb = s.Limbs[legs[l].Index];
                    var ikSettings = ikSet.GetIKSettingsForLimb( limb, s );
                    if( ikSettings != null )
                    {
                        ikSettings.FootDataAnalyze.GroundingCurve = EditorGUILayout.CurveField( legs[l].GetName + " Foot Height:", ikSettings.FootDataAnalyze.GroundingCurve, Color.cyan, new Rect( 0f, 0f, 1f, 1f ) );
                        AnimationDesignerWindow.DrawCurveProgress( optionalBlendGhost );
                    }
                }

                EditorGUILayout.BeginHorizontal();

                EditorGUIUtility.labelWidth = 40;
                firstLegSettings.HeelGroundedTreshold = EditorGUILayout.Slider( "Heel:", firstLegSettings.HeelGroundedTreshold, -2f, 1f );
                firstLegSettings.ToesGroundedTreshold = EditorGUILayout.Slider( "Toes:", firstLegSettings.ToesGroundedTreshold, -2f, 2f );
                EditorGUIUtility.labelWidth = 0;

                if( GUILayout.Button( FGUI_Resources.Tex_Refresh, GUILayout.Height( 18 ) ) )
                {
                    for( int l = 0; l < legs.Count; l++ )
                    {
                        var limb = s.Limbs[legs[l].Index];
                        var ikSettings = ikSet.GetIKSettingsForLimb( limb, s );
                        if( ikSettings != null ) ikSettings.GetClipFootAnalyze( S, limb, _anim_MainSet.settingsForClip, _anim_MainSet, firstLegSettings.HeelGroundedTreshold, firstLegSettings.ToesGroundedTreshold, firstLegSettings.HoldOnGround, _anim_MainSet.ResetRootPosition );
                    }
                }

                EditorGUILayout.EndHorizontal();

                GUILayout.Space( 8 );
            }

            var pDownPower = GVar( "pDown", "Push Down Power:", 0.1f );
            pDownPower.HideFlag = true;
            pDownPower.DrawGUI();

            var lBack = GVar( "lBack", "Look Back:", 0.25f );
            lBack.HideFlag = true;
            lBack.DrawGUI();

            GUILayout.Space( 5 );
            var smoothingMot = GVar( "SM", "Smoothing Motion:", 0.3f );
            smoothingMot.HideFlag = true;
            smoothingMot.SetRangeHelperValue( 0f, 1f );
            smoothingMot.DrawGUI();

            base.InspectorGUI_ModuleBody( optionalBlendGhost, _anim_MainSet, s, cModule, set );
        }

        ADVariable GVar( string id, string displayName, object defValue, string tooltip = "" )
        {
            var v = GetVariable( id, null, defValue );
            v.DisplayName = displayName;
            v.Tooltip = tooltip;
            return v;
        }

        public override void SceneView_DrawSceneHandles( CustomModuleSet customModuleSet, float alphaAnimation = 1, float progress = 0 )
        {
            base.SceneView_DrawSceneHandles( customModuleSet, alphaAnimation, progress );

            if( currentSave == null ) return;
            if( currentSave.LatestAnimator == null ) return;
            if( currentMainSet == null ) return;

            var ikSet = currentSave.GetSetupForClip<ADClipSettings_IK>( currentSave.IKSetupsForClips, currentMainSet.settingsForClip, AnimationDesignerWindow._toSet_SetSwitchToHash );
            if( ikSet == null ) return;

            var lBack = GVar( "lBack", "Look Back:", 0.25f );
            float backProgress = progress - ( lBack.Float * ( 1f / currentMainSet.settingsForClip.length ) );

            for( int l = 0; l < legs.Count; l++ )
            {
                var limb = currentSave.Limbs[legs[l].Index];
                var ikSettings = ikSet.GetIKSettingsForLimb( limb, currentSave );
                if( ikSettings != null )
                {
                    if( ikSettings.WasAnalyzed == false ) continue;
                    if( ikSettings.FootDataAnalyze == null ) continue;

                    Handles.color = ikSettings.FootDataAnalyze.GroundedIn( progress ) ? Color.green : Color.red;
                    Handles.SphereHandleCap( 0, ikSettings.LastTargetIKPosition, Quaternion.identity, 0.1f, EventType.Repaint );

                    Vector3 lookBackPos = currentSave.LatestAnimator.transform.TransformPoint( ikSettings.FootDataAnalyze.GetFootLocalPosition( backProgress ) );
                    Handles.color = ikSettings.FootDataAnalyze.GroundedIn( backProgress ) ? Color.green : Color.red;
                    Handles.DrawDottedLine( lookBackPos, ikSettings.LastTargetIKPosition, 2f );
                    Handles.SphereHandleCap( 0, lookBackPos, Quaternion.identity, 0.05f, EventType.Repaint );

                }
            }
        }

        #endregion


    }
}