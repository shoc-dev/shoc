import { ReactNode } from "react";
import { getByName } from "../../cached-workspace-actions";
import { getJobByLocalId, getJobPermissionsByLocalId } from "../cached-job-actions";
import ErrorScreen from "@/components/error/error-screen";
import JobProvider from "@/providers/job/job-provider";
import JobAccessProvider from "@/providers/job-access/job-access-provider";

export default async function SingleJobLayoutLayout(props: { children: ReactNode, params: Promise<any> }) {
    const params = await props.params;

    const {
        workspaceName,
        jobLocalId
    } = params;

    const {
        children
    } = props;

    const workspace = await getByName(workspaceName);

    if (workspace.errors) {
        return <ErrorScreen errors={workspace.errors} />
    }

    const [job, permissions] = await Promise.all([getJobByLocalId(workspace.data.id, jobLocalId), getJobPermissionsByLocalId(workspace.data.id, jobLocalId)])

    if (job.errors || permissions.errors) {
        return <ErrorScreen errors={job.errors || permissions.errors} />
    }

    return <JobProvider initialValue={job.data}>
        <JobAccessProvider permissions={permissions.data || []}>
            {children}
        </JobAccessProvider>
    </JobProvider>
}
