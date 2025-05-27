import { ReactNode } from "react";
import { getJobByLocalId } from "../../../cached-job-actions";
import ErrorScreen from "@/components/error/error-screen";
import { getByName } from "@/app/workspaces/[workspaceName]/cached-workspace-actions";
import { getTaskBySequence } from "../cached-job-task-actions";
import JobTaskProvider from "@/providers/job-task/job-task-provider";

export default async function SingleJobTaskLayoutLayout(props: { children: ReactNode, params: Promise<any> }) {
    const params = await props.params;

    const {
        workspaceName,
        jobLocalId,
        sequence
    } = params;

    const {
        children
    } = props;

    const workspace = await getByName(workspaceName);

    if (workspace.errors) {
        return <ErrorScreen errors={workspace.errors} />
    }

    const job = await getJobByLocalId(workspace.data.id, jobLocalId)

    if (job.errors) {
        return <ErrorScreen errors={job.errors} />
    }

    const task = await getTaskBySequence(workspace.data.id, job.data.id, sequence)

    if (task.errors) {
        return <ErrorScreen errors={task.errors} />
    }

    return <JobTaskProvider initialValue={task.data}>
        {children}
    </JobTaskProvider>
}
