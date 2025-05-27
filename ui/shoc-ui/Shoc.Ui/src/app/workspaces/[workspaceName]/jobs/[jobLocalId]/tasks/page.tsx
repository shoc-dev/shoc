import { redirect } from "next/navigation";

export default async function JobTasksPage(props: any) {
  const params = await props.params;

  const {
    workspaceName,
    jobLocalId
  } = params;

   redirect(`/workspaces/${workspaceName}/jobs/${jobLocalId}/tasks/0`)
}
