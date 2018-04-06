<%@ Page Language="C#" AutoEventWireup="true"  CodeFile="Default.aspx.cs" Inherits="_Default" MasterPageFile="~/Site.master" Title="Hotel Room Booking Tutorial" %>
<%@ Register Assembly="DayPilot" Namespace="DayPilot.Web.Ui" TagPrefix="DayPilot" %>
<asp:Content ID="HeaderContent" runat="server" ContentPlaceHolderID="HeadContent">
	<script type="text/javascript" src="js/modal.js"></script>
    <link href='css/main.css' type="text/css" rel="stylesheet" /> 
	<script type="text/javascript">

	    var modal = new DayPilot.Modal();
	    modal.border = "10px solid #ccc";
	    modal.closed = function () {
	        if (this.result == "OK") {
	            dps.commandCallBack('refresh');
	        }
	    };

	    function createEvent(start, end, resource) {
	        modal.height = 250;
	        modal.showUrl("New.aspx?start=" + start.toStringSortable() + "&end=" + end.toStringSortable() + "&r=" + resource);
	    }

	    function editEvent(id) {
	        modal.height = 300;
	        modal.showUrl("Edit.aspx?id=" + id);
	    }

	    function afterRender(data) {
	    };

	    function filter(property, value) {
	        if (!dps.clientState.filter) {
	            dps.clientState.filter = {};
	        }
	        if (dps.clientState.filter[property] != value) { // only refresh when the value has changed
	            dps.clientState.filter[property] = value;
	            dps.commandCallBack('filter');
	        }
	    }
	
	</script>
    <style type="text/css">
        .scheduler_default_rowheader .scheduler_default_rowheader_inner 
        {
            border-right: 1px solid #aaa;
        }
        .scheduler_default_rowheader.scheduler_default_rowheadercol2
        {
            background: White;
        }
        .scheduler_default_rowheadercol2 .scheduler_default_rowheader_inner 
        {
            top: 2px;
            bottom: 2px;
            left: 2px;
            background-color: transparent;
            border-left: 5px solid #1a9d13; /* green */
            border-right: 0px none;
        }
        .status_dirty.scheduler_default_rowheadercol2 .scheduler_default_rowheader_inner
        {
            border-left: 5px solid #ea3624; /* red */
        }
        .status_cleanup.scheduler_default_rowheadercol2 .scheduler_default_rowheader_inner
        {
            border-left: 5px solid #f9ba25; /* orange */
        }
    </style>	
</asp:Content>

<asp:Content ID="BodyContent" runat="server" ContentPlaceHolderID="MainContent">
    <div style="margin-bottom: 5px;">
    Show rooms:
        <asp:DropDownList ID="DropDownListFilter" runat="server" onchange="filter('room', this.value)">
        <asp:ListItem Text="All" Value="0"></asp:ListItem>
        <asp:ListItem Text="Single" Value="1"></asp:ListItem>
        <asp:ListItem Text="Double" Value="2"></asp:ListItem>
        <asp:ListItem Text="Triple" Value="3"></asp:ListItem>
        <asp:ListItem Text="Family" Value="4"></asp:ListItem>
        </asp:DropDownList>
    </div>
    <DayPilot:DayPilotScheduler 
        ID="DayPilotScheduler1" 
        runat="server" 
        
        DataStartField="ReservationStart" 
        DataEndField="ReservationEnd" 
        DataTextField="ReservationName" 
        DataValueField="ReservationId" 
        DataResourceField="RoomId" 
        DataTagFields="ReservationStatus"
        
        ClientObjectName="dps"
        
        CellGroupBy="Month"
        CellDuration="1440"
        Days="365"
        
        HeightSpec="Max"
        Height="350"
        Width="100%"
        HeaderFontSize="8pt"
        EventFontSize="8pt"
        
        EventMoveHandling="CallBack" 
        OnEventMove="DayPilotScheduler1_EventMove" 
        
        EventResizeHandling="CallBack"
        OnEventResize="DayPilotScheduler1_EventResize"
        
        TimeRangeSelectedHandling="JavaScript"
        TimeRangeSelectedJavaScript="createEvent(start, end, column);" 
        
        OnCommand="DayPilotScheduler1_Command"
        
        EventClickHandling="JavaScript"
        EventClickJavaScript="editEvent(e.value());" 
        
        AfterRenderJavaScript="afterRender(data);" 

        
        OnBeforeEventRender="DayPilotScheduler1_BeforeEventRender" OnBeforeCellRender="DayPilotScheduler1_BeforeCellRender"

        RowHeaderWidthAutoFit="true"
        EventHeight="50"
        DurationBarVisible="true"
        SyncResourceTree="false"
        
        OnBeforeResHeaderRender="DayPilotScheduler1_BeforeResHeaderRender"

        >
        <TimeHeaders>
            <DayPilot:TimeHeader GroupBy="Month" Format="MMMM yyyy" />
            <DayPilot:TimeHeader GroupBy="Day" />
        </TimeHeaders>
        <HeaderColumns>
            <DayPilot:RowHeaderColumn Title="Room" Width="80" />
            <DayPilot:RowHeaderColumn Title="Size" Width="80" />
            <DayPilot:RowHeaderColumn Title="Status" Width="80" />

        </HeaderColumns>
    </DayPilot:DayPilotScheduler>

    <br />

</asp:Content>