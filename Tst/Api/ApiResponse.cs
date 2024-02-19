using System;
using System.Collections.Generic;

namespace Quake.Api;

public record class ApiResponse(
    ApiStatus Status = ApiStatus.Success,
    Dictionary<String, object>? Data = null,
    string? Message = null
    );
