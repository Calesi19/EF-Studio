import { http, HttpResponse } from "msw";

const API_BASE = "/efstudio/api";

export const defaultContextsResponse = {
  contexts: [
    {
      name: "AppDbContext",
      displayName: "App Db Context",
      isSelected: true,
      isDefault: true,
      isAvailable: true,
      activationError: null,
    },
  ],
};

export const defaultSchemaResponse = [
  {
    key: "Users",
    name: "Users",
    modelName: "User",
    schema: null,
    columns: [
      {
        name: "Id",
        dataType: "int",
        isPrimaryKey: true,
        isNullable: false,
        isForeignKey: false,
        isGeneratedOnAdd: true,
        isEditableOnCreate: false,
      },
      {
        name: "Name",
        dataType: "nvarchar",
        isPrimaryKey: false,
        isNullable: false,
        isForeignKey: false,
        isGeneratedOnAdd: false,
        isEditableOnCreate: true,
      },
      {
        name: "Email",
        dataType: "nvarchar",
        isPrimaryKey: false,
        isNullable: true,
        isForeignKey: false,
        isGeneratedOnAdd: false,
        isEditableOnCreate: true,
      },
    ],
  },
];

export const defaultTableDataResponse = {
  key: "Users",
  name: "Users",
  schema: null,
  page: 1,
  pageSize: 10,
  totalRows: 2,
  rows: [
    { Id: 1, Name: "Alice", Email: "alice@example.com" },
    { Id: 2, Name: "Bob", Email: null },
  ],
};

export const handlers = [
  http.get(`${API_BASE}/contexts`, () => {
    return HttpResponse.json(defaultContextsResponse);
  }),

  http.post(`${API_BASE}/contexts/select`, () => {
    return HttpResponse.json({ selectedContext: "AppDbContext" });
  }),

  http.get(`${API_BASE}/schema`, () => {
    return HttpResponse.json(defaultSchemaResponse);
  }),

  http.get(`${API_BASE}/data`, () => {
    return HttpResponse.json(defaultTableDataResponse);
  }),

  http.post(`${API_BASE}/data`, () => {
    return HttpResponse.json({
      tableKey: "Users",
      createdCount: 1,
      records: [{ Id: 3, Name: "Charlie", Email: null }],
    });
  }),

  http.put(`${API_BASE}/data`, () => {
    return HttpResponse.json({ tableKey: "Users", updatedCount: 1 });
  }),

  http.delete(`${API_BASE}/data`, () => {
    return HttpResponse.json({ tableKey: "Users", deletedCount: 1 });
  }),
];
