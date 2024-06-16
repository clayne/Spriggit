﻿using System.Data;
using System.IO.Abstractions;
using Mutagen.Bethesda.Plugins;
using Noggog;
using Noggog.IO;
using Noggog.WorkEngine;
using Spriggit.Core;

namespace Spriggit.Engine.Services.Singletons;

public class GetMetaToUse
{
    private readonly IFileSystem _fileSystem;
    private readonly IWorkDropoff? _workDropoff;
    private readonly ICreateStream? _createStream;
    private readonly GetDefaultEntryPoint _getDefaultEntryPoint;
    private readonly SpriggitExternalMetaPersister _externalMetaPersister;

    public GetMetaToUse(
        IFileSystem fileSystem,
        IWorkDropoff? workDropoff,
        ICreateStream? createStream,
        GetDefaultEntryPoint getDefaultEntryPoint,
        SpriggitExternalMetaPersister externalMetaPersister)
    {
        _fileSystem = fileSystem;
        _workDropoff = workDropoff;
        _createStream = createStream;
        _getDefaultEntryPoint = getDefaultEntryPoint;
        _externalMetaPersister = externalMetaPersister;
    }
    
    public async Task<SpriggitEmbeddedMeta> Get(
        SpriggitSource? source,
        DirectoryPath spriggitPluginPath,
        CancellationToken cancel)
    {
        var sourceInfo = _externalMetaPersister.TryParseEmbeddedMeta(spriggitPluginPath);

        if (sourceInfo == null)
        {
            var entryPt = await _getDefaultEntryPoint.Get(spriggitPluginPath, cancel);
            sourceInfo = await entryPt.TryGetMetaInfo(
                spriggitPluginPath,
                _workDropoff, 
                _fileSystem, 
                _createStream,
                cancel);
        }
        if (sourceInfo == null) throw new DataException($"Could not locate source info from {spriggitPluginPath}");

        return new SpriggitEmbeddedMeta(
            ModKey: sourceInfo.ModKey,
            Source: source ?? sourceInfo.Source,
            Release: sourceInfo.Release);
    }
}