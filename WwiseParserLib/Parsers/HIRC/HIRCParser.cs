﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WwiseParserLib.Structures.Objects.HIRC;
using WwiseParserLib.Structures.Objects.HIRC.Structs;
using WwiseParserLib.Structures.Sections;

namespace WwiseParserLib.Parsers.HIRC
{
    public static class HIRCParser
    {
        public static HIRCSection Parse(byte[] blob)
        {
            using (var reader = new BinaryReader(new MemoryStream(blob)))
            {
                var hircSection = new HIRCSection(blob.Length);
                hircSection.ObjectCount = reader.ReadUInt32();
                hircSection.Objects = new HIRCObject[hircSection.ObjectCount];
                var objects = new Dictionary<byte, int>();
                for (var i = 0; i < hircSection.ObjectCount; i++)
                {
                    var objectType = reader.ReadByte();
                    var objectLength = reader.ReadUInt32();
                    var objectBlob = reader.ReadBytes((int)objectLength);

                    HIRCObject hircObject;
                    switch ((HIRCObjectType)objectType)
                    {
                        case HIRCObjectType.Sound:
                            hircObject = ParseSound(objectBlob);
                            break;

                        case HIRCObjectType.EventAction:
                            hircObject = ParseEventAction(objectBlob);
                            break;

                        case HIRCObjectType.Event:
                            hircObject = ParseEvent(objectBlob);
                            break;

                        case HIRCObjectType.Container:
                            hircObject = ParseContainer(objectBlob);
                            break;

                        case HIRCObjectType.SwitchContainer:
                            hircObject = ParseSwitchContainer(objectBlob);
                            break;

                        case HIRCObjectType.ActorMixer:
                            hircObject = ParseActorMixer(objectBlob);
                            break;

                        case HIRCObjectType.AudioBus:
                            hircObject = ParseAudioBus(objectBlob);
                            break;

                        case HIRCObjectType.BlendContainer:
                            hircObject = ParseBlendContainer(objectBlob);
                            break;

                        case HIRCObjectType.MusicSegment:
                            hircObject = ParseMusicSegment(objectBlob);
                            break;

                        case HIRCObjectType.MusicTrack:
                            hircObject = ParseMusicTrack(objectBlob);
                            break;

                        case HIRCObjectType.MusicSwitchContainer:
                            hircObject = ParseMusicSwitchContainer(objectBlob);
                            break;

                        case HIRCObjectType.Settings:
                            hircObject = ParseSettings(objectBlob);
                            break;

                        default:
                            hircObject = ParseUnknown(objectType, objectBlob);
                            break;
                    }
                    hircSection.Objects[i] = hircObject;
                }

                return hircSection;
            }
        }

        public static AudioBus ParseAudioBus(byte[] data)
        {
            using (var reader = new BinaryReader(new MemoryStream(data)))
            {
                var audioBus = new AudioBus(data.Length);
                audioBus.Id = reader.ReadUInt32();
                audioBus.ParentId = reader.ReadUInt32();
                audioBus.ParameterCount = reader.ReadByte();
                audioBus.ParameterTypes = new AudioParameterType[audioBus.ParameterCount];
                for (var i = 0; i < audioBus.ParameterCount; i++)
                {
                    audioBus.ParameterTypes[i] = (AudioParameterType)reader.ReadByte();
                }
                audioBus.ParameterValues = new float[audioBus.ParameterCount];
                for (var i = 0; i < audioBus.ParameterCount; i++)
                {
                    audioBus.ParameterValues[i] = reader.ReadSingle();
                }
                audioBus.Positioning = (AudioBusPositioningBehavior)reader.ReadByte();
                audioBus.OnPlaybackLimitReached = (AudioBusLimitBehavior)reader.ReadByte();
                audioBus.SoundInstanceLimit = reader.ReadUInt16();
                audioBus.Channel = (AudioBusChannelType)reader.ReadUInt32();
                audioBus.HdrReleaseMode = (AudioBusHdrReleaseMode)reader.ReadByte();
                audioBus.AutoDuckingRecoveryTime = reader.ReadUInt32();
                audioBus.AutoDuckingMaxVolume = reader.ReadSingle();

                audioBus.DuckedBusCount = reader.ReadUInt32();
                audioBus.DuckedBuses = new AudioBusDuckedBus[audioBus.DuckedBusCount];
                for (var i = 0; i < audioBus.DuckedBusCount; i++)
                {
                    AudioBusDuckedBus duckedBus = default;
                    duckedBus.Id = reader.ReadUInt32();
                    duckedBus.Volume = reader.ReadSingle();
                    duckedBus.FadeOut = reader.ReadUInt32();
                    duckedBus.FadeIn = reader.ReadUInt32();
                    duckedBus.CurveShape = (AudioCurveShapeByte)reader.ReadByte();
                    duckedBus.Target = (AudioBusDuckTarget)reader.ReadByte();
                    audioBus.DuckedBuses[i] = duckedBus;
                }

                audioBus.EffectCount = reader.ReadByte();
                if (audioBus.EffectCount > 0)
                {
                    audioBus.BypassedEffects = (AudioBypassedEffects)reader.ReadByte();
                }
                audioBus.Effects = new AudioEffect[audioBus.EffectCount];
                for (var i = 0; i < audioBus.EffectCount; i++)
                {
                    AudioEffect effect = default;
                    effect.Index = reader.ReadByte();
                    effect.Id = reader.ReadUInt32();
                    effect.ShouldUseShareSets = reader.ReadBoolean();
                    effect.IsRendered = reader.ReadBoolean();
                    audioBus.Effects[i] = effect;
                }

                audioBus.UnknownBytes = reader.ReadBytes(6);
                audioBus.RtpcCount = reader.ReadUInt16();
                audioBus.Rtpcs = new AudioRtpc[audioBus.RtpcCount];
                for (var i = 0; i < audioBus.RtpcCount; i++)
                {
                    AudioRtpc rtpc = new AudioRtpc();
                    rtpc.X = reader.ReadUInt32();
                    rtpc.IsMidi = reader.ReadBoolean();
                    rtpc.IsGeneralSettings = reader.ReadBoolean();
                    rtpc.Parameter = (RtpcParameterType)reader.ReadByte();
                    rtpc.UnknownId = reader.ReadUInt32();
                    rtpc.CurveScalingType = (RtpcCurveType)reader.ReadByte();
                    rtpc.PointCount = reader.ReadUInt16();
                    rtpc.Points = new RtpcPoint[rtpc.PointCount];
                    for (var j = 0; j < rtpc.PointCount; j++)
                    {
                        RtpcPoint rtpcPoint = default;
                        rtpcPoint.X = reader.ReadSingle();
                        rtpcPoint.Y = reader.ReadSingle();
                        rtpcPoint.FollowingCurveShape = (AudioCurveShapeByte)reader.ReadByte();
                        rtpcPoint.Unknown = reader.ReadBytes(3);
                        rtpc.Points[j] = rtpcPoint;
                    }
                    audioBus.Rtpcs[i] = rtpc;
                }

                audioBus.StateGroupCount = reader.ReadUInt32();
                audioBus.StateGroups = new AudioStateGroup[audioBus.StateGroupCount];
                for (var i = 0; i < audioBus.StateGroupCount; i++)
                {
                    AudioStateGroup stateGroup = new AudioStateGroup();
                    stateGroup.Id = reader.ReadUInt32();
                    stateGroup.MusicChangeAt = (MusicKeyPointByte)reader.ReadByte();
                    stateGroup.StateWithSettingsCount = reader.ReadUInt16();
                    stateGroup.StatesWithSettings = new AudioStateWithSettings[stateGroup.StateWithSettingsCount];
                    for (var j = 0; j < stateGroup.StateWithSettingsCount; j++)
                    {
                        AudioStateWithSettings AudioStateWithSettings = default;
                        AudioStateWithSettings.StateId = reader.ReadUInt32();
                        AudioStateWithSettings.SettingsId = reader.ReadUInt32();
                        stateGroup.StatesWithSettings[j] = AudioStateWithSettings;
                    }
                    audioBus.StateGroups[i] = stateGroup;
                }

                return audioBus;
            }
        }

        public static Sound ParseSound(byte[] data)
        {
            using (var reader = new BinaryReader(new MemoryStream(data)))
            {
                var sound = new Sound(data.Length);
                sound.Id = reader.ReadUInt32();
                sound.Unknown_04 = reader.ReadByte();
                sound.Unknown_05 = reader.ReadByte();
                sound.Conversion = (SoundConversionType)reader.ReadByte();
                sound.Unknown_07 = reader.ReadByte();
                sound.Source = (SoundSource)reader.ReadByte();
                sound.AudioId = reader.ReadUInt32();
                sound.AudioLength = reader.ReadUInt32();
                sound.AudioType = (SoundType)reader.ReadByte();
                sound.Properties = reader.ReadAudioProperties();
                return sound;
            }
        }

        public static Container ParseContainer(byte[] data)
        {
            using (var reader = new BinaryReader(new MemoryStream(data)))
            {
                var container = new Container(data.Length);
                container.Id = reader.ReadUInt32();
                container.Properties = reader.ReadAudioProperties();
                container.LoopCount = reader.ReadUInt16();
                container.Unknown_1 = reader.ReadUInt32();
                container.TransitionDuration = reader.ReadSingle();
                container.Unknown_2 = reader.ReadBytes(8);
                container.AvoidLastPlayedCount = reader.ReadUInt16();
                container.Transition = (ContainerTransitionType)reader.ReadByte();
                container.Shuffle = reader.ReadBoolean();
                container.PlayType = (ContainerPlayType)reader.ReadByte();
                container.Behavior = (ContainerSequenceBehavior)reader.ReadByte();
                container.ChildCount = reader.ReadUInt32();
                container.ChildIds = new uint[container.ChildCount];
                for (var i = 0; i < container.ChildCount; i++)
                {
                    container.ChildIds[i] = reader.ReadUInt32();
                }
                container.Unknown_3 = reader.ReadUInt16();

                return container;
            }
        }

        public static ActorMixer ParseActorMixer(byte[] data)
        {
            using (var reader = new BinaryReader(new MemoryStream(data)))
            {
                var actorMixer = new ActorMixer(data.Length);
                actorMixer.Id = reader.ReadUInt32();
                actorMixer.Properties = reader.ReadAudioProperties();
                actorMixer.ChildCount = reader.ReadUInt32();
                actorMixer.ChildIds = new uint[actorMixer.ChildCount];
                for (var i = 0; i < actorMixer.ChildCount; i++)
                {
                    actorMixer.ChildIds[i] = reader.ReadUInt32();
                }

                return actorMixer;
            }
        }

        public static SwitchContainer ParseSwitchContainer(byte[] data)
        {
            using (var reader = new BinaryReader(new MemoryStream(data)))
            {
                var switchContainer = new SwitchContainer(data.Length);
                switchContainer.Id = reader.ReadUInt32();
                switchContainer.Properties = reader.ReadAudioProperties();
                switchContainer.SwitchType = (SwitchType)reader.ReadByte();
                switchContainer.GroupId = reader.ReadUInt32();
                switchContainer.DefaultSwitchOrStateId = reader.ReadUInt32();
                switchContainer.Mode = (SwitchContainerMode)reader.ReadByte();
                switchContainer.AssignedChildCount = reader.ReadUInt32();
                switchContainer.AssignedChildIds = new uint[switchContainer.AssignedChildCount];
                for (var i = 0; i < switchContainer.AssignedChildCount; i++)
                {
                    switchContainer.AssignedChildIds[i] = reader.ReadUInt32();
                }
                switchContainer.SwitchOrStateCount = reader.ReadUInt32();
                switchContainer.SwitchOrStates = new SwitchOrState[switchContainer.SwitchOrStateCount];
                for (var i = 0; i < switchContainer.SwitchOrStateCount; i++)
                {
                    SwitchOrState switchOrState = default;
                    switchOrState.Id = reader.ReadUInt32();
                    switchOrState.AssignedChildCount = reader.ReadUInt32();
                    switchOrState.AssignedChildIds = new uint[switchOrState.AssignedChildCount];
                    for (var j = 0; j < switchOrState.AssignedChildCount; j++)
                    {
                        switchOrState.AssignedChildIds[j] = reader.ReadUInt32();
                    }
                    switchContainer.SwitchOrStates[i] = switchOrState;
                }
                switchContainer.ChildCount = reader.ReadUInt32();
                switchContainer.Children = new SwitchChild[switchContainer.ChildCount];
                for (var i = 0; i < switchContainer.ChildCount; i++)
                {
                    SwitchChild switchChild = default;
                    switchChild.Id = reader.ReadUInt32();
                    switchChild.Behavior = (SwitchPlayBehavior)reader.ReadByte();
                    switchChild.Unknown = reader.ReadByte();
                    switchChild.FadeOut = reader.ReadUInt32();
                    switchChild.FadeIn = reader.ReadUInt32();
                    switchContainer.Children[i] = switchChild;
                }

                return switchContainer;
            }
        }

        public static BlendContainer ParseBlendContainer(byte[] data)
        {
            using (var reader = new BinaryReader(new MemoryStream(data)))
            {
                var blendContainer = new BlendContainer(data.Length);
                blendContainer.Id = reader.ReadUInt32();
                blendContainer.Properties = reader.ReadAudioProperties();
                blendContainer.ChildCount = reader.ReadUInt32();
                blendContainer.ChildIds = new uint[blendContainer.ChildCount];
                for (var i = 0; i < blendContainer.ChildCount; i++)
                {
                    blendContainer.ChildIds[i] = reader.ReadUInt32();
                }
                blendContainer.BlendTrackCount = reader.ReadUInt32();
                blendContainer.BlendTracks = new BlendTrack[blendContainer.BlendTrackCount];
                for (var i = 0; i < blendContainer.BlendTrackCount; i++)
                {
                    BlendTrack blendTrack = default;
                    blendTrack.Id = reader.ReadUInt32();
                    blendTrack.RuleCount = reader.ReadUInt16();
                    blendTrack.Rules = new BlendTrackRule[blendTrack.RuleCount];
                    for (var j = 0; j < blendTrack.RuleCount; j++)
                    {
                        BlendTrackRule blendRule = default;
                        blendRule.X = reader.ReadUInt32();
                        blendRule.XType = (BlendTrackXAxisType)reader.ReadByte();
                        blendRule.Unknown_05 = reader.ReadByte();
                        blendRule.YType = (BlendTrackYAxisType)reader.ReadByte();
                        blendRule.UnknownId = reader.ReadUInt32();
                        blendRule.Unknown_0B = reader.ReadByte();
                        blendRule.PointCount = reader.ReadUInt16();
                        blendRule.Points = new MusicCurvePoint[blendRule.PointCount];
                        for (var k = 0; k < blendRule.PointCount; k++)
                        {
                            MusicCurvePoint point = default;
                            point.X = reader.ReadSingle();
                            point.Y = reader.ReadSingle();
                            point.FollowingCurveShape = (AudioCurveShapeUInt)reader.ReadUInt32();
                            blendRule.Points[k] = point;
                        }
                        blendTrack.Rules[j] = blendRule;
                    }
                    blendTrack.CrossfadeId = reader.ReadUInt32();
                    blendTrack.Unknown = reader.ReadByte();
                    blendTrack.ChildCount = reader.ReadUInt32();
                    blendTrack.Children = new BlendTrackChild[blendTrack.ChildCount];
                    for (var j = 0; j < blendTrack.ChildCount; j++)
                    {
                        BlendTrackChild blendTrackChild = default;
                        blendTrackChild.Id = reader.ReadUInt32();
                        blendTrackChild.CrossfadePointCount = reader.ReadUInt32();
                        blendTrackChild.CrossfadePoints = new MusicCurvePoint[blendTrackChild.CrossfadePointCount];
                        for (var k = 0; k < blendTrackChild.CrossfadePointCount; k++)
                        {
                            MusicCurvePoint crossfadePoint = default;
                            crossfadePoint.X = reader.ReadSingle();
                            crossfadePoint.Y = reader.ReadSingle();
                            crossfadePoint.FollowingCurveShape = (AudioCurveShapeUInt)reader.ReadUInt32();
                            blendTrackChild.CrossfadePoints[k] = crossfadePoint;
                        }
                        blendTrack.Children[j] = blendTrackChild;
                    }
                    blendContainer.BlendTracks[i] = blendTrack;
                }

                return blendContainer;
            }
        }

        public static MusicSegment ParseMusicSegment(byte[] data)
        {
            using (var reader = new BinaryReader(new MemoryStream(data)))
            {
                var musicSegment = new MusicSegment(data.Length);
                musicSegment.Id = reader.ReadUInt32();
                musicSegment.MidiBehavior = (MusicMidiBehavior)reader.ReadByte();
                musicSegment.Properties = reader.ReadAudioProperties();
                musicSegment.ChildCount = reader.ReadUInt32();
                musicSegment.ChildIds = new uint[musicSegment.ChildCount];
                for (var i = 0; i < musicSegment.ChildCount; i++)
                {
                    musicSegment.ChildIds[i] = reader.ReadUInt32();
                }
                musicSegment.GridPeriodTime = reader.ReadDouble();
                musicSegment.GridOffsetTime = reader.ReadDouble();
                musicSegment.Tempo = reader.ReadSingle();
                musicSegment.TimeSignatureUpper = reader.ReadByte();
                musicSegment.TimeSignatureLower = reader.ReadByte();
                musicSegment.Unknown = reader.ReadByte();
                musicSegment.StingerCount = reader.ReadUInt32();
                musicSegment.Stingers = new MusicStinger[musicSegment.StingerCount];
                for (var i = 0; i < musicSegment.StingerCount; i++)
                {
                    MusicStinger stinger = default;
                    stinger.TriggerId = reader.ReadUInt32();
                    stinger.SegmentId = reader.ReadUInt32();
                    stinger.PlayAt = (MusicKeyPointUInt)reader.ReadUInt32();
                    stinger.CueId = reader.ReadUInt32();
                    stinger.DoNotRepeatIn = reader.ReadUInt32();
                    stinger.AllowPlayingInNextSegment = reader.ReadUInt32() == 1;
                    musicSegment.Stingers[i] = stinger;
                }
                musicSegment.EndTrimOffset = reader.ReadDouble();
                musicSegment.MusicCueCount = reader.ReadUInt32();
                musicSegment.MusicCues = new MusicCue[musicSegment.MusicCueCount];
                for (var i = 0; i < musicSegment.MusicCueCount; i++)
                {
                    MusicCue musicCue = default;
                    musicCue.Id = reader.ReadUInt32();
                    musicCue.Time = reader.ReadDouble();
                    musicCue.CustomNameLength = reader.ReadUInt32();
                    if (musicCue.CustomNameLength > 0)
                    {
                        musicCue.CustomName = String.Concat(reader.ReadChars((int)musicCue.CustomNameLength - 1));
                        reader.BaseStream.Position++;   // Skip null byte
                    }
                    musicSegment.MusicCues[i] = musicCue;
                }

                return musicSegment;
            }
        }

        public static MusicSwitchContainer ParseMusicSwitchContainer(byte[] data)
        {
            using (var reader = new BinaryReader(new MemoryStream(data)))
            {
                var musicSwitchContainer = new MusicSwitchContainer(data.Length);
                musicSwitchContainer.Id = reader.ReadUInt32();
                musicSwitchContainer.MidiBehavior = (MusicMidiBehavior)reader.ReadByte();
                musicSwitchContainer.Properties = reader.ReadAudioProperties();
                musicSwitchContainer.ChildCount = reader.ReadUInt32();
                musicSwitchContainer.ChildIds = new uint[musicSwitchContainer.ChildCount];
                for (var i = 0; i < musicSwitchContainer.ChildCount; i++)
                {
                    musicSwitchContainer.ChildIds[i] = reader.ReadUInt32();
                }
                musicSwitchContainer.GridPeriodTime = reader.ReadDouble();
                musicSwitchContainer.GridOffsetTime = reader.ReadDouble();
                musicSwitchContainer.Tempo = reader.ReadSingle();
                musicSwitchContainer.TimeSignatureUpper = reader.ReadByte();
                musicSwitchContainer.TimeSignatureLower = reader.ReadByte();
                musicSwitchContainer.Unknown_1 = reader.ReadByte();
                musicSwitchContainer.StingerCount = reader.ReadUInt32();
                musicSwitchContainer.Stingers = new MusicStinger[musicSwitchContainer.StingerCount];
                for (var i = 0; i < musicSwitchContainer.StingerCount; i++)
                {
                    MusicStinger stinger = default;
                    stinger.TriggerId = reader.ReadUInt32();
                    stinger.SegmentId = reader.ReadUInt32();
                    stinger.PlayAt = (MusicKeyPointUInt)reader.ReadUInt32();
                    stinger.CueId = reader.ReadUInt32();
                    stinger.DoNotRepeatIn = reader.ReadUInt32();
                    stinger.AllowPlayingInNextSegment = reader.ReadBoolean();
                    musicSwitchContainer.Stingers[i] = stinger;
                }
                musicSwitchContainer.TransitionCount = reader.ReadUInt32();
                musicSwitchContainer.Transitions = new MusicTransition[musicSwitchContainer.TransitionCount];
                for (var i = 0; i < musicSwitchContainer.TransitionCount; i++)
                {
                    MusicTransition transition = default;
                    transition.Unknown_1 = reader.ReadUInt32();
                    transition.SourceId = reader.ReadUInt32();
                    transition.Unknown_2 = reader.ReadUInt32();
                    transition.DestinationId = reader.ReadUInt32();
                    transition.FadeOutDuration = reader.ReadUInt32();
                    transition.FadeOutCurveShape = (AudioCurveShapeUInt)reader.ReadUInt32();
                    transition.FadeOutOffset = reader.ReadInt32();
                    transition.ExitSourceAt = (MusicKeyPointByte)reader.ReadUInt32();
                    transition.ExitSourceAtCueId = reader.ReadUInt32();
                    transition.PlayPostExit = reader.ReadByte() == 0xFF;
                    transition.FadeInDuration = reader.ReadUInt32();
                    transition.FadeInCurveShape = (AudioCurveShapeUInt)reader.ReadUInt32();
                    transition.FadeInOffset = reader.ReadInt32();
                    transition.CustomCueFilterId = reader.ReadUInt32();
                    transition.JumpToPlaylistItemId = reader.ReadUInt32();
                    transition.DestinationSyncTo = (MusicTransitionSyncTarget)reader.ReadUInt16();
                    transition.PlayPreEntry = reader.ReadByte() == 0xFF;
                    transition.MatchSourceCueName = reader.ReadBoolean();
                    transition.UseTransitionSegment = reader.ReadBoolean();
                    if (transition.UseTransitionSegment)
                    {
                        transition.TransitionSegmentId = reader.ReadUInt32();
                        transition.TransitionFadeInDuration = reader.ReadUInt32();
                        transition.TransitionFadeInCurveShape = (AudioCurveShapeUInt)reader.ReadUInt32();
                        transition.TransitionFadeInOffset = reader.ReadInt32();
                        transition.TransitionFadeOutDuration = reader.ReadUInt32();
                        transition.TransitionFadeOutCurveShape = (AudioCurveShapeUInt)reader.ReadUInt32();
                        transition.TransitionFadeOutOffset = reader.ReadInt32();
                        transition.PlayTransitionPreEntry = reader.ReadByte() == 0xFF;
                        transition.PlayTransitionPostExit = reader.ReadByte() == 0xFF;
                    }
                    musicSwitchContainer.Transitions[i] = transition;
                }
                musicSwitchContainer.ContinueOnSwitchChange = reader.ReadBoolean();
                musicSwitchContainer.GroupCount = reader.ReadUInt32();
                musicSwitchContainer.GroupIds = new uint[musicSwitchContainer.GroupCount];
                for (var i = 0; i < musicSwitchContainer.GroupCount; i++)
                {
                    musicSwitchContainer.GroupIds[i] = reader.ReadUInt32();
                }
                musicSwitchContainer.GroupTypes = new bool[musicSwitchContainer.GroupCount];
                for (var i = 0; i < musicSwitchContainer.GroupCount; i++)
                {
                    musicSwitchContainer.GroupTypes[i] = reader.ReadBoolean();
                }
                musicSwitchContainer.PathSectionLength = reader.ReadUInt32();
                musicSwitchContainer.UseWeighted = reader.ReadBoolean();
                musicSwitchContainer.Paths = reader.ReadPaths(musicSwitchContainer.PathSectionLength, musicSwitchContainer.ChildIds);

                return musicSwitchContainer;
            }
        }

        public static MusicTrack ParseMusicTrack(byte[] data)
        {
            using (var reader = new BinaryReader(new MemoryStream(data)))
            {
                var musicTrack = new MusicTrack(data.Length);
                musicTrack.Id = reader.ReadUInt32();
                musicTrack.Unknown = reader.ReadByte();
                musicTrack.SoundCount = reader.ReadUInt32();
                musicTrack.Sounds = new Sound[musicTrack.SoundCount];
                for (var i = 0; i < musicTrack.SoundCount; i++)
                {
                    var sound = new Sound(0);
                    sound.Unknown_04 = reader.ReadByte();
                    sound.Unknown_05 = reader.ReadByte();
                    sound.Conversion = (SoundConversionType)reader.ReadByte();
                    sound.Unknown_07 = reader.ReadByte();
                    sound.Source = (SoundSource)reader.ReadByte();
                    sound.AudioId = reader.ReadUInt32();
                    sound.AudioLength = reader.ReadUInt32();
                    sound.AudioType = (SoundType)reader.ReadByte();
                    musicTrack.Sounds[i] = sound;
                }
                musicTrack.TimeParameterCount = reader.ReadUInt32();
                musicTrack.TimeParameters = new MusicTrackTimeParameter[musicTrack.TimeParameterCount];
                for (var i = 0; i < musicTrack.TimeParameterCount; i++)
                {
                    MusicTrackTimeParameter timeParameter = default;
                    timeParameter.SubTrackIndex = reader.ReadUInt32();
                    timeParameter.AudioId = reader.ReadUInt32();
                    timeParameter.BeginOffset = reader.ReadDouble();
                    timeParameter.BeginTrimOffset = reader.ReadDouble();
                    timeParameter.EndOffset = reader.ReadDouble();
                    timeParameter.EndTrimOffset = reader.ReadDouble();
                    musicTrack.TimeParameters[i] = timeParameter;
                }
                musicTrack.SubTrackCount = reader.ReadUInt32();
                musicTrack.CurveCount = reader.ReadUInt32();
                musicTrack.Curves = new MusicTrackCurve[musicTrack.CurveCount];
                for (var i = 0; i < musicTrack.CurveCount; i++)
                {
                    MusicTrackCurve curve = default;
                    curve.TimeParameterIndex = reader.ReadUInt32();
                    curve.Type = (MusicFadeCurveType)reader.ReadUInt32();
                    curve.PointCount = reader.ReadUInt32();
                    curve.Points = new MusicCurvePoint[curve.PointCount];
                    for (var j = 0; j < curve.PointCount; j++)
                    {
                        MusicCurvePoint fadePoint = default;
                        fadePoint.X = reader.ReadSingle();
                        fadePoint.Y = reader.ReadSingle();
                        fadePoint.FollowingCurveShape = (AudioCurveShapeUInt)reader.ReadUInt32();
                        curve.Points[j] = fadePoint;
                    }
                    musicTrack.Curves[i] = curve;
                }
                musicTrack.Properties = reader.ReadAudioProperties();
                musicTrack.TrackType = (MusicTrackType)reader.ReadByte();
                if (musicTrack.TrackType == MusicTrackType.Switch)
                {
                    MusicSwitchParameters switchParameters = default;
                    switchParameters.Unknown = reader.ReadByte();
                    switchParameters.GroupId = reader.ReadUInt32();
                    switchParameters.DefaultSwitchOrStateId = reader.ReadUInt32();
                    switchParameters.SubTrackCount = reader.ReadUInt32();
                    switchParameters.AssociatedSwitchOrStateIds = new uint[switchParameters.SubTrackCount];
                    for (var i = 0; i < switchParameters.SubTrackCount; i++)
                    {
                        switchParameters.AssociatedSwitchOrStateIds[i] = reader.ReadUInt32();
                    }
                    switchParameters.FadeOutDuration = reader.ReadUInt32();
                    switchParameters.FadeOutCurveShape = (AudioCurveShapeUInt)reader.ReadUInt32();
                    switchParameters.FadeOutOffset = reader.ReadInt32();
                    switchParameters.ExitSourceAt = (MusicKeyPointByte)reader.ReadUInt32();
                    switchParameters.ExitSourceAtCueId = reader.ReadUInt32();
                    switchParameters.FadeInDuration = reader.ReadUInt32();
                    switchParameters.FadeInCurveShape = (AudioCurveShapeUInt)reader.ReadUInt32();
                    switchParameters.FadeInOffset = reader.ReadInt32();
                    musicTrack.SwitchParameters = switchParameters;
                }
                musicTrack.LookAheadTime = reader.ReadUInt32();

                return musicTrack;
            }
        }

        public static Event ParseEvent(byte[] data)
        {
            using (var reader = new BinaryReader(new MemoryStream(data)))
            {
                var @event = new Event(data.Length);
                @event.Id = reader.ReadUInt32();
                @event.ActionCount = reader.ReadUInt32();
                @event.ActionIds = new uint[@event.ActionCount];
                for (var i = 0; i < @event.ActionCount; i++)
                {
                    @event.ActionIds[i] = reader.ReadUInt32();
                }

                return @event;
            }
        }

        public static EventAction ParseEventAction(byte[] data)
        {
            using (var reader = new BinaryReader(new MemoryStream(data)))
            {
                var eventAction = new EventAction(data.Length);
                eventAction.Id = reader.ReadUInt32();
                eventAction.Scope = (EventActionScope)reader.ReadByte();
                eventAction.ActionType = (EventActionType)reader.ReadByte();
                eventAction.ObjectId = reader.ReadUInt32();
                eventAction.Unknown_06 = reader.ReadByte();
                eventAction.ParameterCount = reader.ReadByte();
                eventAction.ParameterTypes = new AudioParameterType[eventAction.ParameterCount];
                for (var i = 0; i < eventAction.ParameterCount; i++)
                {
                    eventAction.ParameterTypes[i] = (AudioParameterType)reader.ReadByte();
                }
                eventAction.ParameterValues = new float[eventAction.ParameterCount];
                for (var i = 0; i < eventAction.ParameterCount; i++)
                {
                    eventAction.ParameterValues[i] = reader.ReadSingle();
                }
                eventAction.Unknown_08 = reader.ReadByte();
                EventActionSettings settings;
                switch (eventAction.ActionType)
                {
                    case EventActionType.Play:
                        settings = new EventActionPlaySettings();
                        ((EventActionPlaySettings)settings).FadeInCurve = (AudioCurveShapeByte)reader.ReadByte();
                        ((EventActionPlaySettings)settings).ObjectSoundBankId = reader.ReadUInt32();
                        break;

                    default:
                        settings = new EventActionUnknownSettings();
                        ((EventActionUnknownSettings)settings).Blob
                            = reader.ReadBytes(data.Length - (int)reader.BaseStream.Position);
                        break;
                }
                eventAction.Settings = settings;

                return eventAction;
            }
        }

        public static Settings ParseSettings(byte[] data)
        {
            using (var reader = new BinaryReader(new MemoryStream(data)))
            {
                var settings = new Settings(data.Length);
                settings.Id = reader.ReadUInt32();
                settings.ParameterCount = reader.ReadByte();
                settings.ParameterTypes = new AudioParameterType[settings.ParameterCount];
                for (var i = 0; i < settings.ParameterCount; i++)
                {
                    settings.ParameterTypes[i] = (AudioParameterType)reader.ReadByte();
                }
                settings.ParameterValues = new float[settings.ParameterCount];
                for (var i = 0; i < settings.ParameterCount; i++)
                {
                    settings.ParameterValues[i] = reader.ReadSingle();
                }

                return settings;
            }
        }

        public static Unknown ParseUnknown(byte type, byte[] data)
        {
            using (var reader = new BinaryReader(new MemoryStream(data)))
            {
                var unknown = new Unknown((HIRCObjectType)type, data.Length);
                unknown.Id = reader.ReadUInt32();
                unknown.Blob = reader.ReadBytes(data.Length - 4);

                return unknown;
            }
        }

        private static AudioProperties ReadAudioProperties(this BinaryReader reader)
        {
            AudioProperties audioProperties = default;
            audioProperties.OverrideEffects = reader.ReadBoolean();
            audioProperties.EffectCount = reader.ReadByte();
            if (audioProperties.EffectCount > 0)
            {
                audioProperties.BypassedEffects = (AudioBypassedEffects)reader.ReadByte();
            }
            audioProperties.Effects = new AudioEffect[audioProperties.EffectCount];
            for (var i = 0; i < audioProperties.EffectCount; i++)
            {
                AudioEffect effect = default;
                effect.Index = reader.ReadByte();
                effect.Id = reader.ReadUInt32();
                effect.ShouldUseShareSets = reader.ReadBoolean();
                effect.IsRendered = reader.ReadBoolean();
                audioProperties.Effects[i] = effect;
            }
            audioProperties.Unknown_1 = reader.ReadByte();
            audioProperties.OutputBusId = reader.ReadUInt32();
            audioProperties.ParentId = reader.ReadUInt32();
            audioProperties.PlaybackBehavior = (AudioPlaybackBehavior)reader.ReadByte();
            audioProperties.ParameterCount = reader.ReadByte();
            audioProperties.ParameterTypes = new AudioParameterType[audioProperties.ParameterCount];
            for (var i = 0; i < audioProperties.ParameterCount; i++)
            {
                audioProperties.ParameterTypes[i] = (AudioParameterType)reader.ReadByte();
            }
            audioProperties.ParameterValues = new ValueType[audioProperties.ParameterCount];
            for (var i = 0; i < audioProperties.ParameterCount; i++)
            {
                var parameterType = audioProperties.ParameterTypes[i].ToString();
                if (parameterType.EndsWith("_UInt"))
                {
                    audioProperties.ParameterValues[i] = reader.ReadUInt32();
                }
                else if (parameterType.EndsWith("_Int"))
                {
                    audioProperties.ParameterValues[i] = reader.ReadInt32();
                }
                else
                {
                    audioProperties.ParameterValues[i] = reader.ReadSingle();
                }
            }
            audioProperties.Unknown_2 = reader.ReadByte();
            audioProperties.Positioning = (AudioPositioningBehavior)reader.ReadByte();
            if (audioProperties.Positioning.HasFlag(AudioPositioningBehavior.ThreeDimensional))
            {
                audioProperties.IsGameDefined = reader.ReadBoolean();
                audioProperties.AttenuationId = reader.ReadUInt32();
                if (!audioProperties.IsGameDefined)
                {
                    audioProperties.UserDefinedPlaySettings = (AudioUserDefinedPositioningBehavior)reader.ReadByte();
                    audioProperties.TransitionTime = reader.ReadUInt32();
                    audioProperties.ControlPointKeyCount = reader.ReadUInt32();
                    audioProperties.ControlPointKeys = new AudioControlPointKey[audioProperties.ControlPointKeyCount];
                    for (var i = 0; i < audioProperties.ControlPointKeyCount; i++)
                    {
                        AudioControlPointKey controlPointKey = default;
                        controlPointKey.X = reader.ReadSingle();
                        controlPointKey.Z = reader.ReadSingle();
                        controlPointKey.Y = reader.ReadSingle();
                        controlPointKey.Timestamp = reader.ReadUInt32();
                        audioProperties.ControlPointKeys[i] = controlPointKey;
                    }
                    audioProperties.RandomRangeCount = reader.ReadUInt32();
                    audioProperties.RandomRangeUnknowns = new AudioPathRandomUnknown[audioProperties.RandomRangeCount];
                    for (var i = 0; i < audioProperties.RandomRangeCount; i++)
                    {
                        AudioPathRandomUnknown randomRangeUnknown = default;
                        randomRangeUnknown.Unknown_0 = reader.ReadUInt32();
                        randomRangeUnknown.Unknown_4 = reader.ReadUInt32();
                        audioProperties.RandomRangeUnknowns[i] = randomRangeUnknown;
                    }
                    audioProperties.RandomRanges = new AudioPathRandomRange[audioProperties.RandomRangeCount];
                    for (var i = 0; i < audioProperties.RandomRangeCount; i++)
                    {
                        AudioPathRandomRange randomRange = default;
                        randomRange.LeftRight = reader.ReadSingle();
                        randomRange.FrontBack = reader.ReadSingle();
                        randomRange.UpDown = reader.ReadSingle();
                        audioProperties.RandomRanges[i] = randomRange;
                    }
                }
            }
            audioProperties.AuxSendsBehavior = (AudioAuxSendsBehavior)reader.ReadByte();
            if (audioProperties.AuxSendsBehavior.HasFlag(AudioAuxSendsBehavior.OverrideAuxSends))
            {
                audioProperties.AuxiliarySendBusIds = new uint[4];
                audioProperties.AuxiliarySendBusIds[0] = reader.ReadUInt32();
                audioProperties.AuxiliarySendBusIds[1] = reader.ReadUInt32();
                audioProperties.AuxiliarySendBusIds[2] = reader.ReadUInt32();
                audioProperties.AuxiliarySendBusIds[3] = reader.ReadUInt32();
            }
            audioProperties.LimitBehavior = (AudioLimitBehavior)reader.ReadByte();
            audioProperties.VirtualVoiceReturnBehavior = (AudioVirtualVoiceReturnBehavior)reader.ReadByte();
            audioProperties.Unknown_3 = reader.ReadByte();
            audioProperties.Unknown_4 = reader.ReadByte();
            audioProperties.VirtualVoiceBehavior = (AudioVirtualVoiceBehavior)reader.ReadByte();
            audioProperties.HdrSettings = (AudioHdrSettings)reader.ReadByte();
            audioProperties.StateGroupCount = reader.ReadUInt32();
            audioProperties.StateGroups = new AudioStateGroup[audioProperties.StateGroupCount];
            for (var i = 0; i < audioProperties.StateGroupCount; i++)
            {
                AudioStateGroup stateGroup = new AudioStateGroup();
                stateGroup.Id = reader.ReadUInt32();
                stateGroup.MusicChangeAt = (MusicKeyPointByte)reader.ReadByte();
                stateGroup.StateWithSettingsCount = reader.ReadUInt16();
                stateGroup.StatesWithSettings = new AudioStateWithSettings[stateGroup.StateWithSettingsCount];
                for (var j = 0; j < stateGroup.StateWithSettingsCount; j++)
                {
                    AudioStateWithSettings AudioStateWithSettings = default;
                    AudioStateWithSettings.StateId = reader.ReadUInt32();
                    AudioStateWithSettings.SettingsId = reader.ReadUInt32();
                    stateGroup.StatesWithSettings[j] = AudioStateWithSettings;
                }
                audioProperties.StateGroups[i] = stateGroup;
            }
            audioProperties.RtpcCount = reader.ReadUInt16();
            audioProperties.Rtpcs = new AudioRtpc[audioProperties.RtpcCount];
            for (var i = 0; i < audioProperties.RtpcCount; i++)
            {
                AudioRtpc rtpc = new AudioRtpc();
                rtpc.X = reader.ReadUInt32();
                rtpc.IsMidi = reader.ReadBoolean();
                rtpc.IsGeneralSettings = reader.ReadBoolean();
                rtpc.Parameter = (RtpcParameterType)reader.ReadByte();
                rtpc.UnknownId = reader.ReadUInt32();
                rtpc.CurveScalingType = (RtpcCurveType)reader.ReadByte();
                rtpc.PointCount = reader.ReadUInt16();
                rtpc.Points = new RtpcPoint[rtpc.PointCount];
                for (var j = 0; j < rtpc.PointCount; j++)
                {
                    RtpcPoint rtpcPoint = default;
                    rtpcPoint.X = reader.ReadSingle();
                    rtpcPoint.Y = reader.ReadSingle();
                    rtpcPoint.FollowingCurveShape = (AudioCurveShapeByte)reader.ReadByte();
                    rtpcPoint.Unknown = reader.ReadBytes(3);
                    rtpc.Points[j] = rtpcPoint;
                }
                audioProperties.Rtpcs[i] = rtpc;
            }
            return audioProperties;
        }

        private static MusicPathNode ReadPaths(this BinaryReader reader, uint pathSectionLength, uint[] childIds)
        {
            // Assert section integrity
            if (pathSectionLength % 0x0C > 0)
            {
                throw new ArgumentException("PathSectionLength should be exactly multiples of 0x0C.");
            }

            // Read section bytes
            var sectionCount = (int)pathSectionLength / 0x0C;
            var sections = new List<byte[]>(sectionCount);
            while (sectionCount > 0)
            {
                sections.Add(reader.ReadBytes(0x0C));
                sectionCount--;
            }

            // Read root node (index 0)
            return ReadPathElement(sections, childIds, 0) as MusicPathNode;
        }

        private static MusicPathElement ReadPathElement(List<byte[]> sections, uint[] childIds, uint childrenStartAt)
        {
            var section = sections[(int)childrenStartAt];
            var childId = BitConverter.ToUInt32(section, 4);
            if (childIds.Contains(childId))
            {
                // childId is a Music Segment, reached the end
                var endpoint = new MusicPathEndpoint();
                endpoint.FromStateOrSwitchId = BitConverter.ToUInt32(section, 0);
                endpoint.SegmentId = childId;
                endpoint.Weight = BitConverter.ToUInt16(section, 8);
                endpoint.Unknown_0A = BitConverter.ToUInt16(section, 10);
                return endpoint;
            }
            else
            {
                // childId is not an ID, reached a node
                var node = new MusicPathNode();
                node.FromStateOrSwitchId = BitConverter.ToUInt32(section, 0);
                node.ChildrenStartAtIndex = BitConverter.ToUInt16(section, 4);
                node.ChildCount = BitConverter.ToUInt16(section, 6);
                node.Children = new MusicPathElement[node.ChildCount];
                for (uint i = 0; i < node.ChildCount; i++)
                {
                    node.Children[i] = ReadPathElement(sections, childIds, node.ChildrenStartAtIndex + i);
                }
                node.Weight = BitConverter.ToUInt16(section, 8);
                node.Unknown_0A = BitConverter.ToUInt16(section, 10);
                return node;
            }
        }
    }
}
