using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Monads;

namespace Toolbox.Work;

public interface IWorkActivity
{
    Option<Context> Run(Context context);
}
