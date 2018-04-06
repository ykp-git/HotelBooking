' Copyright � 2005 - 2013 Annpoint, s.r.o.
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
Imports System.Web
Imports System.Web.UI

Partial Public Class Edit
	Inherits Page

	Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs)
		Response.Cache.SetCacheability(HttpCacheability.NoCache)

		If Not IsPostBack Then
			Dim dr As DataRow = dbGetEvent(Request.QueryString("id"))

			If dr Is Nothing Then
				Throw New Exception("The event was not found")
			End If

			TextBoxStart.Text = Convert.ToDateTime(dr("ReservationStart")).ToShortDateString()
			TextBoxEnd.Text = Convert.ToDateTime(dr("ReservationEnd")).ToShortDateString()
			TextBoxName.Text = TryCast(dr("ReservationName"), String)

			DropDownListRoom.DataSource = dbGetResources()
			DropDownListRoom.DataTextField = "RoomName"
			DropDownListRoom.DataValueField = "RoomId"
			DropDownListRoom.SelectedValue = Convert.ToString(dr("RoomId"))
			DropDownListRoom.DataBind()

			DropDownListStatus.SelectedValue = Convert.ToString(dr("ReservationStatus"))

			DropDownListPaid.SelectedValue = Convert.ToString(dr("ReservationPaid"))

			TextBoxName.Focus()
		End If
	End Sub
	Protected Sub ButtonOK_Click(ByVal sender As Object, ByVal e As EventArgs)
		Dim start As Date = Convert.ToDateTime(TextBoxStart.Text).Date.AddHours(12)
		Dim [end] As Date = Convert.ToDateTime(TextBoxEnd.Text).Date.AddHours(12)
		Dim name As String = TextBoxName.Text
		Dim resource As String = DropDownListRoom.SelectedValue
		Dim id_Renamed As String = Request.QueryString("id")
		Dim status As Integer = Convert.ToInt32(DropDownListStatus.SelectedValue)
		Dim paid As Integer = Convert.ToInt32(DropDownListPaid.SelectedValue)

		dbUpdateEvent(id_Renamed, start, [end], name, resource, status, paid)
		Modal.Close(Me, "OK")
	End Sub

	Private Function dbGetResources() As DataTable
		Dim da As New SqlDataAdapter("SELECT [RoomId], [RoomName] FROM [Room]", ConfigurationManager.ConnectionStrings("daypilot").ConnectionString)
		Dim dt As New DataTable()
		da.Fill(dt)

		Return dt
	End Function

	Private Function dbGetEvent(ByVal id As String) As DataRow
		Dim da As New SqlDataAdapter("SELECT * FROM [Reservation] WHERE ReservationId = @id", ConfigurationManager.ConnectionStrings("daypilot").ConnectionString)
		da.SelectCommand.Parameters.AddWithValue("id", id)
		Dim dt As New DataTable()
		da.Fill(dt)

		If dt.Rows.Count > 0 Then
			Return dt.Rows(0)
		End If
		Return Nothing
	End Function

	Private Sub dbUpdateEvent(ByVal id As String, ByVal start As Date, ByVal [end] As Date, ByVal name As String, ByVal resource As String, ByVal status As Integer, ByVal paid As Integer)
		Using con As New SqlConnection(ConfigurationManager.ConnectionStrings("daypilot").ConnectionString)
			con.Open()
			Dim cmd As New SqlCommand("UPDATE [Reservation] SET ReservationStart = @start, ReservationEnd = @end, ReservationName = @name, RoomId = @resource, ReservationStatus = @status, ReservationPaid = @paid WHERE ReservationId = @id", con)
			cmd.Parameters.AddWithValue("id", id)
			cmd.Parameters.AddWithValue("start", start)
			cmd.Parameters.AddWithValue("end", [end])
			cmd.Parameters.AddWithValue("name", name)
			cmd.Parameters.AddWithValue("resource", resource)
			cmd.Parameters.AddWithValue("status", status)
			cmd.Parameters.AddWithValue("paid", paid)
			cmd.ExecuteNonQuery()
		End Using
	End Sub

	Private Sub dbDeleteEvent(ByVal id As String)
		Using con As New SqlConnection(ConfigurationManager.ConnectionStrings("daypilot").ConnectionString)
			con.Open()
			Dim cmd As New SqlCommand("DELETE FROM [Reservation] WHERE ReservationId = @id", con)
			cmd.Parameters.AddWithValue("id", id)
			cmd.ExecuteNonQuery()
		End Using
	End Sub

	Protected Sub ButtonCancel_Click(ByVal sender As Object, ByVal e As EventArgs)
		Modal.Close(Me)
	End Sub
	Protected Sub LinkButtonDelete_Click(ByVal sender As Object, ByVal e As EventArgs)
		Dim id_Renamed As String = Request.QueryString("id")
		dbDeleteEvent(id_Renamed)
		Modal.Close(Me, "OK")
	End Sub
End Class
