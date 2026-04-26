import { CreateRecordsDialog } from "@/components/records/CreateRecordsDialog";
import { DataTable } from "@/components/table/DataTable";
import { useStudioContext } from "@/pages/StudioPage/context/StudioContext";

export function StudioWorkspace() {
  const {
    activeTab,
    tables,
    selectedTable,
    currentRows,
    currentTotalPages,
    currentTotalRows,
    jumpToReference,
    changeFilter,
    changePage,
    changePageSize,
    changeSort,
    deleteRows,
    deleteSelectionResetKey,
    createDialogOpen,
    createDraftRows,
    creatingRows,
    openCreateDialog,
    closeCreateDialog,
    addCreateDraftRow,
    removeCreateDraftRow,
    updateCreateDraftRow,
    submitCreateRows,
    pendingEdits,
    savingEdits,
    setCellEdit,
    saveEdits,
    discardEdits,
  } = useStudioContext();

  if (!activeTab || !selectedTable) {
    return null;
  }

  return (
    <>
      <DataTable
        key={activeTab.id}
        columns={selectedTable.columns}
        rows={currentRows}
        totalRows={currentTotalRows}
        totalPages={currentTotalPages}
        filter={activeTab.filter}
        sort={activeTab.sort}
        pagination={activeTab.pagination}
        onFilterChange={changeFilter}
        onSortChange={changeSort}
        onPageChange={changePage}
        onPageSizeChange={changePageSize}
        onAddRecord={openCreateDialog}
        canAddRecord
        selectionResetKey={deleteSelectionResetKey}
        onEditRecord={() => {}}
        onDeleteRecord={() => {}}
        onBulkDelete={deleteRows}
        onJumpToRef={jumpToReference}
        pendingEdits={pendingEdits}
        pendingEditCount={pendingEdits.size}
        savingEdits={savingEdits}
        onCellEdit={setCellEdit}
        onSaveEdits={saveEdits}
        onDiscardEdits={discardEdits}
      />
      <CreateRecordsDialog
        open={createDialogOpen}
        tableDef={selectedTable}
        draftRows={createDraftRows}
        allTables={tables}
        creatingRows={creatingRows}
        onOpenChange={(open) => {
          if (!open) {
            closeCreateDialog();
          }
        }}
        onAddRow={addCreateDraftRow}
        onRemoveRow={removeCreateDraftRow}
        onChangeRow={updateCreateDraftRow}
        onSubmit={submitCreateRows}
      />
    </>
  );
}
