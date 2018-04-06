Imports System.Text
Imports System.Web.Script.Serialization
Imports System.Web.UI


	''' <summary>
	''' Summary description for Modal
	''' </summary>
	Public Class Modal

		Public Shared Sub Close(ByVal page As Page)
			Close(page, Nothing)
		End Sub

		Public Shared Function Script(ByVal result As Object) As String
			Dim sb As New StringBuilder()
			sb.Append("<script type='text/javascript'>")
			sb.Append("if (parent && parent.DayPilot && parent.DayPilot.ModalStatic) {")
			sb.Append("parent.DayPilot.ModalStatic.close(" & (New JavaScriptSerializer()).Serialize(result) & ");")
			sb.Append("}")
			sb.Append("</script>")

			Return sb.ToString()
		End Function

		Public Shared Sub Close(ByVal page As Page, ByVal result As Object)
			page.ClientScript.RegisterStartupScript(GetType(Page), "close", Script(result))
			Return
		End Sub

	End Class
