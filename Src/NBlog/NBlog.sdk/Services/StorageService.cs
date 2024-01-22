﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Toolbox.Azure.DataLake;
using Toolbox.Tools;
using Toolbox.Types;

namespace NBlog.sdk;

public class StorageService
{
    private IDatalakeStore _datalakeStore;
    private ILogger<StorageService> _logger;

    public StorageService(IDatalakeStore datalakeStore, ILogger<StorageService> logger)
    {
        _datalakeStore = datalakeStore.NotNull();
        _logger = logger.NotNull();
    }

    public async Task<Option<DataETag>> GetGet(string fileId, ScopeContext context)
    {
        context = context.With(_logger);

        context.LogInformation("Reading fileId={fileId}", fileId);
        var dataOption = await _datalakeStore.Read(fileId, context);
        if (dataOption.IsError())
        {
            context.Location().LogError("Could not find fileId={fileId}", fileId);
            return (StatusCode.NotFound, "No fileId");
        }

        DataETag data = dataOption.Return();
        if (!data.Validate(out var v)) return v.LogOnError(context, "DataETag").ToOptionStatus<DataETag>();

        return data;
    }

    public async Task<Option<DatalakePathProperties>> GetPathProperties(string fileId, ScopeContext context)
    {
        context = context.With(_logger);
        context.LogInformation("Getting properties of file={file}", fileId);

        Option<DatalakePathProperties> propertyOption = await _datalakeStore.GetPathProperties(fileId, context);
        if (propertyOption.IsError())
        {
            context.LogError("Failed to read path properties for file={file}", fileId);
            return propertyOption;
        }

        return propertyOption;
    }
}
