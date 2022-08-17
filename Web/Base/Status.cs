namespace RestEasy.Web.Base {
    public enum Status {
        OK = 200,
        Created = 201,
        Accpeted = 202,
        BadRequest = 400,
        Unauthorized = 401,
        Forbidden = 403,
        NotFound = 404,
        BadGateway = 502,
        ServiceUnavailable = 503,
        InternalServerError = 500,
        MovedPermanently = 301,
        MovedTemporary = 302,
        NoContent = 204,
        All = -1
    }
}