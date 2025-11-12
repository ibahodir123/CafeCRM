using System;

namespace CafeCrm.Application.DTOs;

public record EventLogDto(DateTime Timestamp, string EventType, string Message, string? Details);
