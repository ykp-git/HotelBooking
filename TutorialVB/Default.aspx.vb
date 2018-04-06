' Copyright � 2005 - 2015 Annpoint, s.r.o.
'   Use of this software is subject to license terms. 
'   http://www.daypilot.org/
'
'   If you have purchased a DayPilot Pro license, you are allowed to use this 
'   code under the conditions of DayPilot Pro License Agreement:
'
'   http://www.daypilot.org/files/LicenseAgreement.pdf
'
'   Otherwise, you are allowed to use it for evaluation purposes only under 
'   the conditions of DayPilot Pro Trial License Agreement:
'   
'   http://www.daypilot.org/files/LicenseAgreementTrial.pdf
'   
'

Imports System
Imports System.Configuration
Imports System.Data
Imports System.Data.SqlClient
Imports System.Drawing
Imports DayPilot.Web.Ui
Imports DayPilot.Web.Ui.Data
Imports DayPilot.Web.Ui.Enums
Imports DayPilot.Web.Ui.Enums.Scheduler
Imports DayPilot.Web.Ui.Events.Scheduler

Partial Public Class _Default
	Inherits System.Web.UI.Page

	Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs)
		' prevent invalid ViewState errors in Firefox        
		If Request.Browser.Browser = "Firefox" Then
			Response.Cache.SetNoStore()
		End If

		DayPilotScheduler1.Separators.Clear()
		DayPilotScheduler1.Separators.Add(Date.Now, Color.Red)

		If Not IsPostBack Then

			'DayPilotScheduler1.StartDate = new DateTime(DateTime.Today.Year, 1, 1);

			DayPilotScheduler1.Scale = TimeScale.Manual
			Dim start As New Date(Date.Today.Year, 1, 1, 12, 0, 0)
			Dim [end] As Date = start.AddYears(1)

			DayPilotScheduler1.Timeline.Clear()
			Dim cell As Date = start
			Do While cell < [end]
				DayPilotScheduler1.Timeline.Add(cell, cell.AddDays(1))
				cell = cell.AddDays(1)
			Loop

			LoadResourcesAndEvents()

			' scroll to this month
			Dim firstOfMonth As New Date(Date.Today.Year, Date.Today.Month, 1)
			DayPilotScheduler1.SetScrollX(firstOfMonth)

		End If
	End Sub

	Protected Sub DayPilotScheduler1_EventMove(ByVal sender As Object, ByVal e As DayPilot.Web.Ui.Events.EventMoveEventArgs)
		Dim id_Renamed As String = e.Value
		Dim start As Date = e.NewStart
		Dim [end] As Date = e.NewEnd
		Dim resource As String = e.NewResource

		Dim message As String = Nothing

		If Not dbIsFree(id_Renamed, start, [end], resource) Then
			message = "The reservation cannot overlap with an existing reservation."
		ElseIf e.OldEnd <= Date.Today Then
			message = "This reservation cannot be changed anymore."
		ElseIf e.OldStart < Date.Today Then
			If e.OldResource <> e.NewResource Then
				message = "The room cannot be changed anymore."
			Else
				message = "The reservation start cannot be changed anymore."
			End If
		ElseIf e.NewStart < Date.Today Then
			message = "The reservation cannot be moved to the past."
		Else
			dbUpdateEvent(id_Renamed, start, [end], resource)
			'message = "Reservation moved.";
		End If

		LoadResourcesAndEvents()
		DayPilotScheduler1.UpdateWithMessage(message)
	End Sub

	Private Function dbGetEvents(ByVal start As Date, ByVal [end] As Date) As DataTable
		Dim da As New SqlDataAdapter("SELECT [ReservationId], [ReservationName], [ReservationStart], [ReservationEnd], [RoomId], [ReservationStatus], [ReservationPaid] FROM [Reservation] WHERE NOT (([ReservationEnd] <= @start) OR ([ReservationStart] >= @end))", ConfigurationManager.ConnectionStrings("daypilot").ConnectionString)
		da.SelectCommand.Parameters.AddWithValue("start", start)
		da.SelectCommand.Parameters.AddWithValue("end", [end])
		Dim dt As New DataTable()
		da.Fill(dt)
		Return dt
	End Function

	Private Sub dbUpdateEvent(ByVal id As String, ByVal start As Date, ByVal [end] As Date, ByVal resource As String)
		Using con As New SqlConnection(ConfigurationManager.ConnectionStrings("daypilot").ConnectionString)
			con.Open()
			Dim cmd As New SqlCommand("UPDATE [Reservation] SET ReservationStart = @start, ReservationEnd = @end, RoomId = @resource WHERE ReservationId = @id", con)
			cmd.Parameters.AddWithValue("id", id)
			cmd.Parameters.AddWithValue("start", start)
			cmd.Parameters.AddWithValue("end", [end])
			cmd.Parameters.AddWithValue("resource", resource)
			cmd.ExecuteNonQuery()
		End Using
	End Sub

	Private Function dbIsFree(ByVal id As String, ByVal start As Date, ByVal [end] As Date, ByVal resource As String) As Boolean
		' event with the specified id will be ignored

		Dim da As New SqlDataAdapter("SELECT count(ReservationId) as count FROM [Reservation] WHERE NOT (([ReservationEnd] <= @start) OR ([ReservationStart] >= @end)) AND RoomId = @resource AND ReservationId <> @id", ConfigurationManager.ConnectionStrings("daypilot").ConnectionString)
		da.SelectCommand.Parameters.AddWithValue("id", id)
		da.SelectCommand.Parameters.AddWithValue("start", start)
		da.SelectCommand.Parameters.AddWithValue("end", [end])
		da.SelectCommand.Parameters.AddWithValue("resource", resource)
		Dim dt As New DataTable()
		da.Fill(dt)

		Dim count As Integer = Convert.ToInt32(dt.Rows(0)("count"))
		Return count = 0
	End Function


	Private Sub LoadResources()
		DayPilotScheduler1.Resources.Clear()

		Dim roomFilter As String = "0"
		If DayPilotScheduler1.ClientState("filter") IsNot Nothing Then
			roomFilter = CStr(DayPilotScheduler1.ClientState("filter")("room"))
		End If

		Dim da As New SqlDataAdapter("SELECT [RoomId], [RoomName], [RoomStatus], [RoomSize] FROM [Room] WHERE RoomSize = @beds or @beds = '0'", ConfigurationManager.ConnectionStrings("daypilot").ConnectionString)
		da.SelectCommand.Parameters.AddWithValue("beds", roomFilter)
		Dim dt As New DataTable()
		da.Fill(dt)

		For Each r As DataRow In dt.Rows
			Dim name As String = DirectCast(r("RoomName"), String)
			Dim id_Renamed As String = Convert.ToString(r("RoomId"))
			Dim status As String = DirectCast(r("RoomStatus"), String)
			Dim beds As Integer = Convert.ToInt32(r("RoomSize"))
			Dim bedsFormatted As String = If(beds = 1, "1 bed", String.Format("{0} beds", beds))

			Dim res As New Resource(name, id_Renamed)
			res.DataItem = r
			res.Columns.Add(New ResourceColumn(bedsFormatted))
			res.Columns.Add(New ResourceColumn(status))

			DayPilotScheduler1.Resources.Add(res)
		Next r

	End Sub

	Protected Sub DayPilotScheduler1_Command(ByVal sender As Object, ByVal e As DayPilot.Web.Ui.Events.CommandEventArgs)
		Select Case e.Command
			Case "refresh"
				LoadResourcesAndEvents()
			Case "filter"
				LoadResourcesAndEvents()
		End Select
	End Sub

	Private Sub LoadResourcesAndEvents()
		LoadResources()
		DayPilotScheduler1.DataSource = dbGetEvents(DayPilotScheduler1.VisibleStart, DayPilotScheduler1.VisibleEnd)
		DayPilotScheduler1.DataBind()
		DayPilotScheduler1.Update()
	End Sub

	Protected Sub DayPilotScheduler1_EventResize(ByVal sender As Object, ByVal e As DayPilot.Web.Ui.Events.EventResizeEventArgs)
		Dim id_Renamed As String = e.Value
		Dim start As Date = e.NewStart
		Dim [end] As Date = e.NewEnd
		Dim resource As String = e.Resource

		Dim message As String = Nothing

		If Not dbIsFree(id_Renamed, start, [end], resource) Then
			message = "The reservation cannot overlap with an existing reservation."
		ElseIf e.OldEnd <= Date.Today Then
			message = "This reservation cannot be changed anymore."
		ElseIf e.OldStart <> e.NewStart Then
			If e.OldStart < Date.Today Then
			   message = "The reservation start cannot be changed anymore."
			ElseIf e.NewStart < Date.Today Then
				message = "The reservation cannot be moved to the past."
			End If
		Else
			dbUpdateEvent(id_Renamed, start, [end], resource)
			'message = "Reservation updated.";
		End If

		LoadResourcesAndEvents()
		DayPilotScheduler1.UpdateWithMessage(message)
	End Sub
	Protected Sub DayPilotScheduler1_BeforeEventRender(ByVal sender As Object, ByVal e As DayPilot.Web.Ui.Events.Scheduler.BeforeEventRenderEventArgs)
		e.InnerHTML = String.Format("{0} ({1:d} - {2:d})", e.Text, e.Start, e.End)
		Dim status As Integer = Convert.ToInt32(e.Tag("ReservationStatus"))

		Select Case status
			Case 0 ' new
				If e.Start < Date.Today.AddDays(2) Then ' must be confirmed two day in advance
					e.DurationBarColor = "red"
					e.ToolTip = "Expired (not confirmed in time)"
				Else
					e.DurationBarColor = "orange"
					e.ToolTip = "New"
				End If
			Case 1 ' confirmed
				If e.Start < Date.Today OrElse (e.Start = Date.Today AndAlso Date.Now.TimeOfDay.Hours > 18) Then ' must arrive before 6 pm
					e.DurationBarColor = "#f41616" ' red
					e.ToolTip = "Late arrival"
				Else
					e.DurationBarColor = "green"
					e.ToolTip = "Confirmed"
				End If
			Case 2 ' arrived
				If e.End < Date.Today OrElse (e.End = Date.Today AndAlso Date.Now.TimeOfDay.Hours > 11) Then ' must checkout before 10 am
					e.DurationBarColor = "#f41616" ' red
					e.ToolTip = "Late checkout"
				Else
					e.DurationBarColor = "#1691f4" ' blue
					e.ToolTip = "Arrived"
				End If
			Case 3 ' checked out
				e.DurationBarColor = "gray"
				e.ToolTip = "Checked out"
			Case Else
				Throw New ArgumentException("Unexpected status.")
		End Select

		e.InnerHTML = e.InnerHTML & String.Format("<br /><span style='color:gray'>{0}</span>", e.ToolTip)


		Dim paid As Integer = Convert.ToInt32(e.DataItem("ReservationPaid"))
		Dim paidColor As String = "#aaaaaa"

		e.Areas.Add((New Area()).Bottom(10).Right(4).Html("<div style='color:" & paidColor & "; font-size: 8pt;'>Paid: " & paid & "%</div>").Visibility(AreaVisibility.Visible))
		e.Areas.Add((New Area()).Left(4).Bottom(8).Right(4).Height(2).Html("<div style='background-color:" & paidColor & "; height: 100%; width:" & paid & "%'></div>").Visibility(AreaVisibility.Visible))
	End Sub

	Protected Sub DayPilotScheduler1_BeforeCellRender(ByVal sender As Object, ByVal e As DayPilot.Web.Ui.Events.BeforeCellRenderEventArgs)
		If e.IsBusiness Then
			e.BackgroundColor = "#ffffff"
		Else
			e.BackgroundColor = "#f8f8f8"
		End If
	End Sub

	Protected Sub DayPilotScheduler1_BeforeResHeaderRender(ByVal sender As Object, ByVal e As BeforeResHeaderRenderEventArgs)
		Dim status As String = CStr(e.DataItem("RoomStatus"))
		Select Case status
			Case "Dirty"
				e.CssClass = "status_dirty"
			Case "Cleanup"
				e.CssClass = "status_cleanup"
		End Select
	End Sub
End Class
