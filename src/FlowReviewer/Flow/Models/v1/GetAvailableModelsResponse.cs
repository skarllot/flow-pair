using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace Ciandt.FlowTools.FlowReviewer.Flow.Models.v1;

public sealed record GetAvailableModelsResponse(
    string Id,
    string Name,
    string Provider,
    string Directory,
    string Family,
    BaseRemoteConfig RemoteConfig,
    string Hash,
    ImmutableList<string> Capabilities);

[JsonDerivedType(typeof(RemoteConfigMod1))]
[JsonDerivedType(typeof(RemoteConfigMod2))]
public abstract record BaseRemoteConfig(
    string Region);

public sealed record RemoteConfigMod1(
    string Region,
    string DeploymentId,
    int? Tpm,
    int? MaxTokens)
    : BaseRemoteConfig(Region);

public sealed record RemoteConfigMod2(
    string Region,
    string Version,
    string ProjectId,
    string ClientEmail,
    int? InputTokens)
    : BaseRemoteConfig(Region);
