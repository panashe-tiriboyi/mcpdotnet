# IrisDMS API Documentation

## Authentication

The API uses Azure AD JWT Bearer authentication. All requests (except for the webhook endpoint) require a valid JWT token in the Authorization header.

### Authentication Header
```
Authorization: Bearer {your_jwt_token}
```

### Required Token Claims
- The token must be a valid Azure AD JWT token
- The token must not be expired
- The token must contain the necessary scopes/permissions

## API Endpoints

### Dossier Controller
Base path: `/api/iris/dossier`

#### Get Informatieobject
```http
GET /api/iris/dossier/informatieobjecten/{id}
```
**Authorization**: Required  
**Parameters**:
- `id` (path, required): The ID of the informatieobject
- Query Parameters:
  - `relatieId` (required): The relation ID
  - `namens` (required): The name
  - `container` (optional): The container name

#### List Informatieobjecten
```http
GET /api/iris/dossier/informatieobjecten
```
**Authorization**: Required  
**Parameters**:
- Query Parameters:
  - `relatieId` (required): The relation ID
  - `isExternePublicatie` (optional): Boolean flag for external publication
  - `namens` (required): The name
  - `zaaknummer` (optional): Case number
  - `referentie` (optional): Reference

#### Create Informatieobject
```http
POST /api/iris/dossier/informatieobjecten
```
**Authorization**: Required  
**Body**: InformatieobjectPostDTO

### EDMS Controller
Base path: `/api/edms`

#### Handle Webhook
```http
POST /api/edms/webhook/{tenantId}
```
**Authorization**: Not Required (Anonymous)  
**Parameters**:
- `tenantId` (path, required): The tenant ID
- `validationtoken` (query, optional): Validation token for webhook setup
- Request body: Webhook payload

#### Get Count
```http
GET /api/edms/count/{id}
```
**Authorization**: Required  
**Parameters**:
- `id` (path, required): The ID to count

#### Get EDMS Informatieobject
```http
GET /api/edms/informatieobjecten/{id}
```
**Authorization**: Required  
**Parameters**:
- `id` (path, required): The ID of the informatieobject
- Query Parameters:
  - `container` (optional): The container name
  - `skip` (optional, default: 0): Number of records to skip
  - `pageSize` (optional, default: 50): Number of records per page

#### List EDMS Informatieobjecten
```http
GET /api/edms/informatieobjecten
```
**Authorization**: Required  
**Parameters**:
- Query Parameters:
  - `container` (optional): The container name
  - `skip` (optional, default: 0): Number of records to skip
  - `pageSize` (optional, default: 50): Number of records per page

#### Get Containers
```http
GET /api/edms/container
```
**Authorization**: Required  

## Error Handling

The API uses standard HTTP status codes:

- 200: Success
- 400: Bad Request - Invalid input parameters
- 401: Unauthorized - Missing or invalid authentication
- 403: Forbidden - Valid authentication but insufficient permissions
- 404: Not Found - Resource not found
- 500: Internal Server Error - Server-side error

## Rate Limiting

Please be mindful of API usage and implement appropriate retry mechanisms with exponential backoff for failed requests.

## Additional Notes

1. All endpoints require tenant configuration through the `RequireTenantConfigAttribute`
2. The API uses memory caching for improved performance on certain endpoints
3. Swagger UI is available at `/swagger` when running in development mode
4. API supports both HTTP and HTTPS protocols (HTTPS recommended for production)
