using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Tools;

namespace Toolbox.Azure.Table;

public class TableStore
{
    private readonly TableOption _tableOption;
    private readonly ILogger<TableStore> _logger;

    public TableStore(TableOption tableOption, ILogger<TableStore> logger)
    {
        _tableOption = tableOption.VerifyNotNull(nameof(tableOption));
        _logger = logger.VerifyNotNull(nameof(logger));

        //var account = CloudStorageAccount
    }


}
