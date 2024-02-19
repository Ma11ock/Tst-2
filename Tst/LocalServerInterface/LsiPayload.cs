using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Quake.Lsi;

public abstract partial record LsiPayload;

public sealed partial record LsiServerReady : LsiPayload;

public sealed partial record LsiPerformance(double fps, long memUsage) : LsiPayload;
