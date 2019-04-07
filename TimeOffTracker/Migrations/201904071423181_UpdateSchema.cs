namespace TimeOffTracker.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateSchema : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.RequestChecks", "Approver_Id", "dbo.AspNetUsers");
            DropIndex("dbo.RequestChecks", new[] { "Approver_Id" });
            RenameColumn(table: "dbo.RequestChecks", name: "Approver_Id", newName: "ApproverId");
            RenameColumn(table: "dbo.RequestChecks", name: "Request_Id", newName: "RequestId");
            RenameColumn(table: "dbo.RequestChecks", name: "Status_Id", newName: "StatusId");
            RenameIndex(table: "dbo.RequestChecks", name: "IX_Request_Id", newName: "IX_RequestId");
            RenameIndex(table: "dbo.RequestChecks", name: "IX_Status_Id", newName: "IX_StatusId");
            AlterColumn("dbo.RequestChecks", "ApproverId", c => c.String(maxLength: 128));
            CreateIndex("dbo.RequestChecks", "ApproverId");
            AddForeignKey("dbo.RequestChecks", "ApproverId", "dbo.AspNetUsers", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.RequestChecks", "ApproverId", "dbo.AspNetUsers");
            DropIndex("dbo.RequestChecks", new[] { "ApproverId" });
            AlterColumn("dbo.RequestChecks", "ApproverId", c => c.String(nullable: false, maxLength: 128));
            RenameIndex(table: "dbo.RequestChecks", name: "IX_StatusId", newName: "IX_Status_Id");
            RenameIndex(table: "dbo.RequestChecks", name: "IX_RequestId", newName: "IX_Request_Id");
            RenameColumn(table: "dbo.RequestChecks", name: "StatusId", newName: "Status_Id");
            RenameColumn(table: "dbo.RequestChecks", name: "RequestId", newName: "Request_Id");
            RenameColumn(table: "dbo.RequestChecks", name: "ApproverId", newName: "Approver_Id");
            CreateIndex("dbo.RequestChecks", "Approver_Id");
            AddForeignKey("dbo.RequestChecks", "Approver_Id", "dbo.AspNetUsers", "Id", cascadeDelete: true);
        }
    }
}
