[Back to README](../README.md)

### Sales

Sales records use **External Identities** for Customer, Branch, and Product: each reference stores a `Guid` id and a denormalized `name` (description) from the source domain.

List endpoints follow the conventions in [General API](./general-api.md) (pagination, ordering, filtering).

#### Business rules (discounts)

Discounts apply **per line item** (per product quantity):

| Quantity | Discount |
|----------|----------|
| 1–4 | 0% |
| 5–9 | 10% |
| 10–20 | 20% |
| > 20 | Rejected (`400 Bad Request`) |

Line total formula: `quantity × unitPrice × (1 - discountPercent / 100)`, rounded to 2 decimal places.

#### Domain events

The application logs the following domain events (no message broker required):

- `SaleCreated`
- `SaleModified`
- `SaleCancelled`
- `ItemCancelled`

---

#### GET /api/sales

- Description: Retrieve a paginated list of sales
- Query Parameters:
  - `_page` (optional): Page number (default: `1`)
  - `_size` (optional): Page size (default: `10`)
  - `_order` (optional): Sort order — `saleNumber`, `saleDate`, `totalAmount`, `status` (e.g. `saleDate desc`, `totalAmount asc`)
  - `SaleNumber` (optional): Filter by sale number; use `*` for prefix match (e.g. `SALE*`)
  - `Customer` (optional): Filter by customer name (partial match with `*`)
  - `Branch` (optional): Filter by branch name (partial match with `*`)
  - `Status` (optional): `Active` or `Cancelled`
  - `_minSaleDate` / `_maxSaleDate` (optional): Sale date range
  - `_minTotalAmount` / `_maxTotalAmount` (optional): Total amount range
- Response:
  ```json
  {
    "data": [
      {
        "id": "uuid",
        "saleNumber": "string",
        "saleDate": "datetime",
        "customer": { "id": "uuid", "name": "string" },
        "branch": { "id": "uuid", "name": "string" },
        "status": "string (Active | Cancelled)",
        "totalAmount": "number",
        "items": [
          {
            "id": "uuid",
            "product": { "id": "uuid", "name": "string" },
            "quantity": "integer",
            "unitPrice": "number",
            "discountPercent": "number",
            "lineTotal": "number",
            "isCancelled": "boolean"
          }
        ],
        "createdAt": "datetime",
        "updatedAt": "datetime | null"
      }
    ],
    "totalItems": "integer",
    "currentPage": "integer",
    "totalPages": "integer"
  }
  ```

Example:

```
GET /api/sales?_page=1&_size=10&_order=saleDate desc&SaleNumber=SALE*&Customer=Acme
```

---

#### POST /api/sales

- Description: Create a new sale. Discounts are calculated automatically from item quantities.
- Request Body:
  ```json
  {
    "saleNumber": "string",
    "saleDate": "datetime",
    "customer": { "id": "uuid", "name": "string" },
    "branch": { "id": "uuid", "name": "string" },
    "items": [
      {
        "product": { "id": "uuid", "name": "string" },
        "quantity": "integer",
        "unitPrice": "number"
      }
    ]
  }
  ```
- Response (`201 Created`):
  ```json
  {
    "success": true,
    "message": "Sale created successfully",
    "data": { "...": "SaleResponse — see GET /api/sales/{id}" },
    "errors": []
  }
  ```
- Errors:
  - `400` — validation failure or duplicate `saleNumber`
  - `400` — quantity above 20 (`MaxQuantityExceededException`)

---

#### GET /api/sales/{id}

- Description: Retrieve a sale by id
- Path Parameters:
  - `id`: Sale id (`uuid`)
- Response (`200 OK`):
  ```json
  {
    "success": true,
    "message": "Sale retrieved successfully",
    "data": { "...": "SaleResponse" },
    "errors": []
  }
  ```
- Errors:
  - `404` — sale not found

---

#### PUT /api/sales/{id}

- Description: **Full replace** of an active sale (date, customer, branch, and all line items). Existing line items are replaced; discounts are recalculated.
- Path Parameters:
  - `id`: Sale id (`uuid`)
- Request Body:
  ```json
  {
    "saleDate": "datetime",
    "customer": { "id": "uuid", "name": "string" },
    "branch": { "id": "uuid", "name": "string" },
    "items": [
      {
        "product": { "id": "uuid", "name": "string" },
        "quantity": "integer",
        "unitPrice": "number"
      }
    ]
  }
  ```
- Response (`200 OK`):
  ```json
  {
    "success": true,
    "message": "Sale updated successfully",
    "data": { "...": "SaleResponse" },
    "errors": []
  }
  ```
- Errors:
  - `400` — validation or domain error (e.g. sale already cancelled, quantity above 20)
  - `404` — sale not found

---

#### PATCH /api/sales/{id}/cancel

- Description: Cancel an entire sale. Sets status to `Cancelled` and `totalAmount` to `0`.
- Path Parameters:
  - `id`: Sale id (`uuid`)
- Request Body: none
- Response (`200 OK`):
  ```json
  {
    "success": true,
    "message": "Sale cancelled successfully",
    "data": { "...": "SaleResponse" },
    "errors": []
  }
  ```
- Errors:
  - `400` — sale already cancelled
  - `404` — sale not found

---

#### PATCH /api/sales/{id}/items/{itemId}/cancel

- Description: Cancel a single line item on an active sale. Recalculates `totalAmount` from remaining active items.
- Path Parameters:
  - `id`: Sale id (`uuid`)
  - `itemId`: Line item id (`uuid`)
- Request Body: none
- Response (`200 OK`):
  ```json
  {
    "success": true,
    "message": "Sale item cancelled successfully",
    "data": { "...": "SaleResponse" },
    "errors": []
  }
  ```
- Errors:
  - `400` — sale cancelled, item not found, or item already cancelled
  - `404` — sale not found

---

#### DELETE /api/sales/{id}

- Description: Permanently delete a sale and its line items
- Path Parameters:
  - `id`: Sale id (`uuid`)
- Response (`200 OK`):
  ```json
  {
    "success": true,
    "message": "Sale deleted successfully",
    "errors": []
  }
  ```
- Errors:
  - `404` — sale not found

---

#### Error response format

Domain and not-found errors follow [General API — Error Handling](./general-api.md#error-handling):

```json
{
  "type": "DomainError",
  "error": "DomainError",
  "detail": "Sale item '...' was not found."
}
```

Validation errors from FluentValidation return `400 Bad Request` with a validation error payload from the API pipeline.

<br/>
<div style="display: flex; justify-content: space-between;">
  <a href="./general-api.md">Previous: General API</a>
  <a href="./products-api.md">Next: Products API</a>
</div>
