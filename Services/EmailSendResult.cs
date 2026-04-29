namespace ArlianTrans.Web.Services;

public record EmailSendResult(bool Success, string? ErrorMessage = null);
