"use client"

import { Button } from "@/components/ui/button"
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from "@/components/ui/dialog"
import { zodResolver } from "@hookform/resolvers/zod"
import { useIntl } from "react-intl"
import { z } from "zod"
import ErrorAlert from "@/components/general/error-alert"
import { useEffect, useState } from "react"
import { FieldValues, SubmitHandler, useForm } from "react-hook-form"
import { Form, FormControl, FormField, FormItem, FormLabel, FormMessage } from "@/components/ui/form"
import SpinnerIcon from "@/components/icons/spinner-icon"
import { rpc } from "@/server-actions/rpc"
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select"
import { ModalDialogProps } from "@/components/general/component-types"
import { workspaceRoles, workspaceRolesMap } from "@/well-known/workspace"

interface DialogProps extends ModalDialogProps {
  workspaceId: string,
  item: any
}

export default function WorkspaceMemberUpdateDialog({ item, workspaceId, open, trigger, onClose, onSuccess }: DialogProps) {

  const intl = useIntl();
  const [errors, setErrors] = useState<any[]>([]);
  const [progress, setProgress] = useState(false);

  const formSchema = z.object({
    role: z.custom(role => workspaceRolesMap[role], intl.formatMessage({ id: 'workspaces.members.validation.invalidRole' })),
  });

  const form = useForm({
    resolver: zodResolver(formSchema),
    shouldUseNativeValidation: false
  })

  const onOk: SubmitHandler<FieldValues> = async (values) => {

    setErrors([]);
    setProgress(true);

    const { data, errors } = await rpc('workspace/user-workspace-members/updateById', {
      workspaceId: item.workspaceId,
      id: item.id,
      input: {
        role: values.role
      }
    });

    setProgress(false);

    if (errors) {
      setErrors(errors);
      return;
    }

    if (onSuccess) {
      onSuccess(data)
    }

    onOpenChangeWrapper(false)
  }

  const onOpenChangeWrapper = (openValue: boolean): void => {
    setErrors([]);

    if (!openValue && onClose) {
      onClose();
    }
  }

  useEffect(() => {

    if(!open){
      return;
    }

    if(item){
      form.reset({...item})
    } 
    else {
      form.reset()
    }
  }, [form, open, item]);

  return <Dialog open={open} onOpenChange={onOpenChangeWrapper} modal>
    <DialogTrigger asChild>
      {trigger}
    </DialogTrigger>
    <DialogContent className="w-4/5 md:w-1/2">
      <DialogHeader>
        <DialogTitle>{intl.formatMessage({ id: 'workspaces.members.update' })}</DialogTitle>
        <DialogDescription>
          {intl.formatMessage({ id: 'workspaces.members.updateNotice' })}
        </DialogDescription>
      </DialogHeader>
      <ErrorAlert errors={errors} title={intl.formatMessage({ id: 'workspaces.members.updateError' })} />
      <Form {...form}>
        <form onSubmit={form.handleSubmit(onOk)}>
          <div className="grid gap-2">
            <div className="grid grap-1">
              <FormField
                control={form.control}
                name="role"
                render={({ field: { ref, ...fieldNoRef } }) => (
                  <FormItem>
                    <FormLabel>{intl.formatMessage({ id: 'workspaces.labels.role' })}</FormLabel>
                    <Select onValueChange={fieldNoRef.onChange} {...fieldNoRef}>
                      <FormControl>
                        <SelectTrigger>
                          <SelectValue placeholder={intl.formatMessage({ id: 'workspaces.members.placeholders.role' })} />
                        </SelectTrigger>
                      </FormControl>
                      <SelectContent>
                        {workspaceRoles.map((item) => <SelectItem key={item.key} value={item.key}>{intl.formatMessage({ id: item.display })}</SelectItem>)}
                      </SelectContent>
                    </Select>
                  </FormItem>
                )}
              />
            </div>
            <DialogFooter>
              <Button type="submit" disabled={progress}>
                {progress && (
                  <SpinnerIcon className="mr-2 h-4 w-4 animate-spin" />
                )}
                {intl.formatMessage({ id: 'global.actions.update' })}
              </Button>
            </DialogFooter>
          </div>
        </form>
      </Form>

    </DialogContent>
  </Dialog>
}